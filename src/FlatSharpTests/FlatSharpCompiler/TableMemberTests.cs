﻿/*
 * Copyright 2020 James Courtney
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace FlatSharpTests.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using FlatSharp;
    using FlatSharp.Attributes;
    using FlatSharp.Compiler;
    using FlatSharp.TypeModel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TableMemberTests
    {
        [TestMethod]
        public void TableMember_bool() => this.RunCompoundTestWithDefaultValue_Bool("bool");

        [TestMethod]
        public void TableMember_byte() => this.RunCompoundTestWithDefaultValue<byte>("ubyte");

        [TestMethod]
        public void TableMember_sbyte() => this.RunCompoundTestWithDefaultValue<sbyte>("byte");

        [TestMethod]
        public void TableMember_short_alias() => this.RunCompoundTestWithDefaultValue<short>("short");

        [TestMethod]
        public void TableMember_short() => this.RunCompoundTestWithDefaultValue<short>("int16");

        [TestMethod]
        public void TableMember_ushort_alias() => this.RunCompoundTestWithDefaultValue<ushort>("ushort");

        [TestMethod]
        public void TableMember_ushort() => this.RunCompoundTestWithDefaultValue<ushort>("uint16");

        [TestMethod]
        public void TableMember_int_alias() => this.RunCompoundTestWithDefaultValue<int>("int");

        [TestMethod]
        public void TableMember_int() => this.RunCompoundTestWithDefaultValue<int>("int32");

        [TestMethod]
        public void TableMember_uint_alias() => this.RunCompoundTestWithDefaultValue<uint>("uint");

        [TestMethod]
        public void TableMember_uint() => this.RunCompoundTestWithDefaultValue<uint>("uint32");

        [TestMethod]
        public void TableMember_long_alias() => this.RunCompoundTestWithDefaultValue<long>("long");

        [TestMethod]
        public void TableMember_long() => this.RunCompoundTestWithDefaultValue<long>("int64");

        [TestMethod]
        public void TableMember_ulong_alias() => this.RunCompoundTestWithDefaultValue<ulong>("ulong");

        [TestMethod]
        public void TableMember_ulong() => this.RunCompoundTestWithDefaultValue<ulong>("uint64");

        [TestMethod]
        public void TableMember_float_alias() => this.RunCompoundTestWithDefaultValue<float>("float", "G17");

        [TestMethod]
        public void TableMember_float() => this.RunCompoundTestWithDefaultValue<float>("float32", "G17");

        [TestMethod]
        public void TableMember_double_alias() => this.RunCompoundTestWithDefaultValue<double>("double", "G17");

        [TestMethod]
        public void TableMember_double() => this.RunCompoundTestWithDefaultValue<double>("float64", "G17");

        [TestMethod]
        public void TableMember_string() => this.RunCompoundTest<string>("string");

        private void RunCompoundTestWithDefaultValue_Bool(string fbsType)
        {
            this.RunSingleTest<bool>($"{fbsType} = true", hasDefaultValue: true, expectedDefaultValue: true);
            this.RunSingleTest<bool>($"{fbsType} = false", hasDefaultValue: true, expectedDefaultValue: false);
            this.RunCompoundTest<bool>(fbsType);
        }

        private void RunCompoundTestWithDefaultValue<T>(string fbsType, string format = null) where T : struct, IFormattable
        {
            Random random = new Random();
            byte[] data = new byte[16];
            random.NextBytes(data);
            T randomValue = MemoryMarshal.Cast<byte, T>(data)[0];

            this.RunSingleTest<T>($"{fbsType} = {randomValue.ToString(format, null).ToLowerInvariant()}", hasDefaultValue: true, expectedDefaultValue: randomValue);

            this.RunCompoundTest<T>(fbsType);
        }

        private void RunCompoundTest<T>(string fbsType)
        {
            this.RunSingleTest<T>(fbsType);
            this.RunSingleTest<T>($"{fbsType} (deprecated)", deprecated: true);
            this.RunSingleTest<IList<T>>($"[{fbsType}]");
            this.RunSingleTest<IList<T>>($"[{fbsType}]  (vectortype: IList)");
            this.RunSingleTest<T[]>($"[{fbsType}]  (vectortype: Array)");
            this.RunSingleTest<IReadOnlyList<T>>($"[{fbsType}]  (vectortype: IReadOnlyList)");

            if (typeof(T).IsValueType)
            {
                this.RunSingleTest<Memory<T>>($"[{fbsType}]  (vectortype: Memory)");
                this.RunSingleTest<ReadOnlyMemory<T>>($"[{fbsType}]  (vectortype: ReadOnlyMemory)");
            }
            else
            {
                Assert.ThrowsException<InvalidFlatBufferDefinitionException>(() => this.RunSingleTest<Memory<T>>($"[{fbsType}]  (vectortype: Memory)"));
                Assert.ThrowsException<InvalidFlatBufferDefinitionException>(() => this.RunSingleTest<ReadOnlyMemory<T>>($"[{fbsType}]  (vectortype: ReadOnlyMemory)"));
            }
        }

        private void RunSingleTest<T>(string fbsType, bool deprecated = false, bool hasDefaultValue = false, T expectedDefaultValue = default)
        {
            try
            {
                string schema = $@"namespace TableMemberTests; table Table {{ member:{fbsType}; member2:int; }}";
                Assembly asm = FlatSharpCompiler.CompileAndLoadAssembly(schema);

                Type tableType = asm.GetType("TableMemberTests.Table");
                PropertyInfo property = tableType.GetProperty("member");

                Assert.AreEqual(typeof(T), property.PropertyType);
                var attribute = property.GetCustomAttribute<FlatBufferItemAttribute>();

                Assert.AreEqual(0, attribute.Index);
                Assert.AreEqual(deprecated, attribute.Deprecated);

                Assert.AreEqual(hasDefaultValue, attribute.DefaultValue != null);
                if (hasDefaultValue)
                {
                    Assert.IsInstanceOfType(attribute.DefaultValue, typeof(T));

                    T actualDefault = (T)attribute.DefaultValue;
                    Assert.AreEqual(0, Comparer<T>.Default.Compare(expectedDefaultValue, actualDefault));
                }
                
                byte[] data = new byte[100];
                CompilerTestHelpers.CompilerTestSerializer.ReflectionSerialize(Activator.CreateInstance(tableType), data);
                CompilerTestHelpers.CompilerTestSerializer.ReflectionParse(tableType, data);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }
    }
}
