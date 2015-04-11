﻿using System;
using System.Collections.Generic;
using FullSerializer;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Vexe.Runtime.Serialization
{ 
    public class FullSerializerBackend : SerializerBackend
    {
        private fsSerializer serializer;

        public FullSerializerBackend()
        {
            serializer = new fsSerializer();
            serializer.AddConverter(new UnityObjectConverter());
            serializer.AddConverter(new MethodInfoConverter());

            Logic = VFWSerializationLogic.Instance;

            fsConfig.SerializeAttributes = Logic.Attributes.SerializeMember;
            fsConfig.IgnoreSerializeAttributes = Logic.Attributes.DontSerializeMember;
        }

        public override string Serialize(Type type, object graph, object context)
        {
            serializer.Context.Set(context as List<UnityObject>);

            fsData data;
            var fail = serializer.TrySerialize(type, graph, out data);
            if (fail.Failed) throw new Exception(fail.FormattedMessages);

            return fsJsonPrinter.CompressedJson(data);
        }

        public override object Deserialize(Type type, string serializedState, object context)
        {
            fsData data;
            fsResult status = fsJsonParser.Parse(serializedState, out data);
            if (status.Failed) throw new Exception(status.FormattedMessages);

            serializer.Context.Set(context as List<UnityObject>);

            object deserialized = null;
            status = serializer.TryDeserialize(data, type, ref deserialized);
            if (status.Failed) throw new Exception(status.FormattedMessages);
            return deserialized;
        }
    }
}
