﻿/*
 * Copyright 2018 James Courtney
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

namespace FlatSharp
{
    using System;
    using System.Text;

    /// <summary>
    /// An implementation of <see cref="InputBuffer"/> for managed arrays.
    /// </summary>
    public sealed class ArrayInputBuffer : InputBuffer
    {
        private readonly ArraySegment<byte> memory;

        public override int Length => this.memory.Count;

        public ArrayInputBuffer(ArraySegment<byte> memory)
        {
            this.memory = memory;
        }

        public ArrayInputBuffer(byte[] buffer) : this(new ArraySegment<byte>(buffer))
        {
        }

        public override byte ReadByte(int offset)
        {
            return ScalarSpanReader.ReadByte(this.memory.AsSpan().Slice(offset));
        }

        public override sbyte ReadSByte(int offset)
        {
            return ScalarSpanReader.ReadSByte(this.memory.AsSpan().Slice(offset));
        }

        public override ushort ReadUShort(int offset)
        {
            CheckAlignment(offset, sizeof(ushort));
            return ScalarSpanReader.ReadUShort(this.memory.AsSpan().Slice(offset));
        }

        public override short ReadShort(int offset)
        {
            CheckAlignment(offset, sizeof(short));
            return ScalarSpanReader.ReadShort(this.memory.AsSpan().Slice(offset));
        }

        public override uint ReadUInt(int offset)
        {
            CheckAlignment(offset, sizeof(uint));
            return ScalarSpanReader.ReadUInt(this.memory.AsSpan().Slice(offset));
        }

        public override int ReadInt(int offset)
        {
            CheckAlignment(offset, sizeof(int));
            return ScalarSpanReader.ReadInt(this.memory.AsSpan().Slice(offset));
        }

        public override ulong ReadULong(int offset)
        {
            CheckAlignment(offset, sizeof(ulong));
            return ScalarSpanReader.ReadULong(this.memory.AsSpan().Slice(offset));
        }

        public override long ReadLong(int offset)
        {
            CheckAlignment(offset, sizeof(long));
            return ScalarSpanReader.ReadLong(this.memory.AsSpan().Slice(offset));
        }

        public override float ReadFloat(int offset)
        {
            CheckAlignment(offset, sizeof(float));
            return ScalarSpanReader.ReadFloat(this.memory.AsSpan().Slice(offset));
        }

        public override double ReadDouble(int offset)
        {
            CheckAlignment(offset, sizeof(double));
            return ScalarSpanReader.ReadDouble(this.memory.AsSpan().Slice(offset));
        }

        protected override string ReadStringProtected(int offset, int byteLength, Encoding encoding)
        {
            return ScalarSpanReader.ReadString(this.memory.AsSpan().Slice(offset, byteLength), encoding);
        }

        protected override Memory<byte> GetByteMemory(int start, int length)
        {
            return new Memory<byte>(this.memory.Array, this.memory.Offset + start, length);
        }

        protected override ReadOnlyMemory<byte> GetReadOnlyByteMemory(int start, int length)
        {
            return this.GetByteMemory(start, length);
        }
    }
}
