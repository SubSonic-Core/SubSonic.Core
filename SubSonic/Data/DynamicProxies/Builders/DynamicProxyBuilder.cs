﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;

namespace SubSonic.Data.DynamicProxies
{
    internal class DynamicProxyBuilder
    {
        private readonly TypeBuilder typeBuilder;
        private readonly Type baseType;

        private FieldBuilder fieldDbContextAccessor;

        public DynamicProxyBuilder(TypeBuilder typeBuilder, Type baseType)
        {
            this.typeBuilder = typeBuilder ?? throw new ArgumentNullException(nameof(typeBuilder));
            this.baseType = baseType;
        }

        public Type CreateType()
        {
            BuildProxyConstructor();

            foreach (PropertyInfo property in baseType.GetProperties())
            {
                if (!property.GetMethod.IsVirtual)
                {
                    continue;
                }

                BuildOverriddenProperty(property.Name, property.PropertyType);
            }

            return typeBuilder.CreateType();
        }

        private void BuildProxyConstructor()
        {
            fieldDbContextAccessor = typeBuilder.DefineField($"_dbContextAccessor", typeof(DbContextAccessor), FieldAttributes.Private);

            ConstructorInfo baseCtor = baseType.GetConstructor(Type.EmptyTypes);

            ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                CallingConventions.Standard,
                new Type[] { typeof(DbContextAccessor) });

            ILGenerator iLGenerator = constructorBuilder.GetILGenerator();

            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Call, baseCtor);
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldarg_1);
            iLGenerator.Emit(OpCodes.Stfld, fieldDbContextAccessor);
            iLGenerator.Emit(OpCodes.Ret);
        }

        private void BuildOverriddenProperty(string propertyName, Type propertyType)
        {
            FieldBuilder propertyField = typeBuilder.DefineField($"_{propertyName}", propertyType, FieldAttributes.Private);

            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(
                    propertyName,
                    PropertyAttributes.None,
                    propertyType,
                    Type.EmptyTypes);

            MethodAttributes methodAttributesForGetAndSet = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual ;

            MethodBuilder
                getMethod = typeBuilder.DefineMethod($"get_{propertyName}", methodAttributesForGetAndSet, propertyType, Type.EmptyTypes),
                setMethod = typeBuilder.DefineMethod($"set_{propertyName}", methodAttributesForGetAndSet, null, new Type[] { propertyType });

            ILGenerator
                iLGetGenerator = getMethod.GetILGenerator(),
                iLSetGenerator = setMethod.GetILGenerator();

            Type internalExt = typeof(InternalExtensions);

            MethodInfo
                load = fieldDbContextAccessor.FieldType.GetMethods()
                    .Where(method =>
                        method.Name == (propertyType.IsClass ? "Load" : "LoadCollection"))
                    .Single().MakeGenericMethod(baseType),
                isNull = internalExt.GetMethod("IsNull", BindingFlags.Public | BindingFlags.Static , null, new[] { typeof(object) }, null);

            Label fieldIsNotNull = iLGetGenerator.DefineLabel();

            LocalBuilder
                propertyInfo = iLGetGenerator.DeclareLocal(typeof(PropertyInfo));

            iLGetGenerator.Emit(OpCodes.Ldarg_0);                   // this
            iLGetGenerator.Emit(OpCodes.Ldfld, propertyField);      // propertyField
            iLGetGenerator.EmitCall(OpCodes.Call, isNull, null);    // use the static extension method IsNull
            iLGetGenerator.Emit(OpCodes.Brfalse_S, fieldIsNotNull); // value is not null
            //{
                iLGetGenerator.Emit(OpCodes.Ldarg_0);                                                                               // this
                iLGetGenerator.EmitCall(OpCodes.Call, typeof(object).GetMethod("GetType"), null);                                   // call GetType method
                iLGetGenerator.Emit(OpCodes.Ldstr, propertyName);                                                                   // push new string of propertyName
                iLGetGenerator.EmitCall(OpCodes.Call, typeof(Type).GetMethod("GetProperty", new[] { typeof(string) }), null);       // call GetProperty with the propertyName as the parameter
                iLGetGenerator.Emit(OpCodes.Stloc, propertyInfo);                                                                   // store PropertyInfo object in the local variable propertyInfo

                iLGetGenerator.Emit(OpCodes.Ldarg_0);                           // this ptr as the first parameter
                iLGetGenerator.Emit(OpCodes.Ldfld, fieldDbContextAccessor);     // field variable _dbContextAccessor
                iLGetGenerator.Emit(OpCodes.Ldloc, propertyInfo);               // local variable propertyInfo as the second parameter
                iLGetGenerator.Emit(OpCodes.Ldarg_0);                           // this
                iLGetGenerator.EmitCall(OpCodes.Call, load, null);              // call the Load or LoadCollection on the DBContextAccessor object
                iLGetGenerator.Emit(OpCodes.Stfld, propertyField);              // store the return in the propertyField
            //}
            iLGetGenerator.MarkLabel(fieldIsNotNull);               // jump here when propertyField is not null
            iLGetGenerator.Emit(OpCodes.Ldarg_0);   // this
            iLGetGenerator.Emit(OpCodes.Ldfld, propertyField); // propertyField
            iLGetGenerator.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(getMethod, baseType.GetProperty(propertyName).GetMethod);

            iLSetGenerator.Emit(OpCodes.Ldarg_0);
            iLSetGenerator.Emit(OpCodes.Ldarg_1);
            iLSetGenerator.Emit(OpCodes.Stfld, propertyField);
            iLSetGenerator.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(setMethod, baseType.GetProperty(propertyName).SetMethod);
        }
    }
}