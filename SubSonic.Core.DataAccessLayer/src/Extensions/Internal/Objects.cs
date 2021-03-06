﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SubSonic
{
    using Linq;
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// warning: some methods in this extension class are referenced at runtime and the reference count at design time will not reflect this.
    /// </remarks>
    internal static partial class InternalExtensions
    {
        public static bool IsIntGreaterThan(this object left, object right)
        {
            if (left is int _left)
            {
                if (right is int _right)
                {
                   return _left > _right;
                }
            }

            return false;
        }
        public static bool IsOfType<TType>(this object source)
        {
            return IsOfType(source, typeof(TType));
        }

        public static bool IsOfType(this object source, Type type)
        {
            Type sourceType = source.GetType();

            return sourceType == type || sourceType.IsSubclassOf(type);
        }

        public static IEnumerable<TType> Convert<TType>(this IEnumerable array)
        {
            List<TType> result = new List<TType>();

            foreach(object obj in array)
            {
                if (obj.IsOfType<TType>())
                {
                    result.Add((TType)obj);
                }
            }

            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <remarks>referneced in dynamic proxy</remarks>
        public static bool IsNull(this object source)
        {
            return source == null;
        }

        public static TType IsNull<TType>(this TType source, TType @default = default(TType))
        {
            if(SubSonicQueryable.IsNull(source))
            {
                return @default;
            }
            return source;
        }

        public static TReturn IsNotNull<TType, TReturn>(this TType source, Func<TType, TReturn> selector, TReturn @default = default(TReturn))
        {
            if (SubSonicQueryable.IsNotNull(source))
            {
                return selector(source);
            }
            return @default;
        }

        public static void IsNotNull<TType>(this TType source, Action<TType> action)
        {
            if (SubSonicQueryable.IsNotNull(source))
            {
                action(source);
            }
        }

        public static TType IsNullThrow<TType>(this TType source, Exception exception)
        {
            if (SubSonicQueryable.IsNull(source))
            {
                throw exception;
            }
            return source;
        }

        public static TType IsNullThrowArgumentNull<TType>(this TType source, string name)
        {
            return IsNullThrow(source, new ArgumentNullException(name));
        }

        

        public static bool IsDefaultValue<TType>(this TType left)
        {
            return IsDefaultValue(left, typeof(TType));
        }

        public static bool IsDefaultValue(this object left, Type type)
        {
            return left.Equals(GetDefault(type));
        }


    }
}
