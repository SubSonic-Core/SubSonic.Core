﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace SubSonic
{
    using SubSonic.Core.DAL.src;
    public class MissingServiceCollectionException
        : Exception
    {
        public MissingServiceCollectionException()
            : this(SubSonicErrorMessages.MissingServiceCollectionException)
        {
        }

        public MissingServiceCollectionException(string message) : base(message)
        {
        }

        public MissingServiceCollectionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MissingServiceCollectionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
