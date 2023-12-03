﻿using Prowl.Runtime.Serialization;
using System;
using System.IO;
using System.Linq;

namespace Prowl.Runtime.Serializer
{
    /// <summary>
    /// This class is responsible for converting CompoundTags to and from binary data.
    /// Binary data is not human-readable, so bad for git, and doesn't work well with versioning, but it is much more compact than text data.
    /// Works great for sending data over the network, or for standalone builds.
    /// </summary>
    public static class BinaryTagConverter
    {

        #region Writing
        public static void WriteToFile(CompoundTag tag, FileInfo file)
        {
            using var stream = file.OpenWrite();
            using var writer = new BinaryWriter(stream);
            WriteTo(tag, writer);
        }

        public static void WriteTo(CompoundTag tag, BinaryWriter writer) => WriteCompound(tag, writer);

        private static void WriteCompound(CompoundTag tag, BinaryWriter writer)
        {
            writer.Write(tag.SerializedType);
            writer.Write(tag.SerializedID);
            writer.Write(tag.Name);
            writer.Write(tag.AllTags.Count());
            foreach (var subTag in tag.AllTags)
                WriteTag(subTag, true, writer); // Compounds always need tag names
        }

        private static void WriteTag(Tag tag, bool writeName, BinaryWriter writer)
        {
            var type = tag.GetTagType();
            writer.Write((byte)type);
            writer.Write(writeName); // HasName
            if (writeName)
                writer.Write(tag.Name);
            if (type == TagType.Byte) writer.Write(tag.ByteValue);
            else if (type == TagType.Short) writer.Write(tag.ShortValue);
            else if (type == TagType.Int) writer.Write(tag.IntValue);
            else if (type == TagType.Long) writer.Write(tag.LongValue);
            else if (type == TagType.Float) writer.Write(tag.FloatValue);
            else if (type == TagType.Double) writer.Write(tag.DoubleValue);
            else if (type == TagType.String) writer.Write(tag.StringValue);
            else if (type == TagType.Null) { } // Nothing for Null
            else if (type == TagType.ByteArray)
            {
                writer.Write(tag.ByteArrayValue.Length);
                writer.Write(tag.ByteArrayValue);
            }
            else if (type == TagType.List)
            {
                var listTag = (ListTag)tag;
                writer.Write((byte)listTag.ListType);
                writer.Write(listTag.Count);
                foreach (var subTag in listTag.Tags)
                    WriteTag(subTag, false, writer); // Lists dont care about names, so dont need to write Tag Names inside a List
            }
            else if (type == TagType.Compound) WriteCompound((CompoundTag)tag, writer);
            else throw new Exception($"Unknown tag type: {type}");
        }

        #endregion


        #region Reading
        public static CompoundTag ReadFromFile(FileInfo file)
        {
            using var stream = file.OpenRead();
            using var reader = new BinaryReader(stream);
            return ReadFrom(reader);
        }

        public static CompoundTag ReadFrom(BinaryReader reader) => ReadCompound(reader);

        private static CompoundTag ReadCompound(BinaryReader reader)
        {
            CompoundTag tag = new();
            tag.SerializedType = reader.ReadString();
            tag.SerializedID = reader.ReadInt32();
            tag.Name = reader.ReadString();
            var tagCount = reader.ReadInt32();
            for (int i = 0; i < tagCount; i++)
                tag.Add(ReadTag(reader));
            return tag;
        }

        private static Tag ReadTag(BinaryReader reader)
        {
            var type = (TagType)reader.ReadByte();
            var hasName = reader.ReadBoolean();
            string name = "";
            if (hasName)
                name = reader.ReadString();
            if (type == TagType.Byte) return new ByteTag(name, reader.ReadByte());
            else if (type == TagType.Short) return new ShortTag(name, reader.ReadInt16());
            else if (type == TagType.Int) return new IntTag(name, reader.ReadInt32());
            else if (type == TagType.Long) return new LongTag(name, reader.ReadInt64());
            else if (type == TagType.Float) return new FloatTag(name, reader.ReadSingle());
            else if (type == TagType.Double) return new DoubleTag(name, reader.ReadDouble());
            else if (type == TagType.String) return new StringTag(name, reader.ReadString());
            else if (type == TagType.Null) return new NullTag();
            else if (type == TagType.ByteArray) return new ByteArrayTag(name, reader.ReadBytes(reader.ReadInt32()));
            else if (type == TagType.List)
            {
                var listType = (TagType)reader.ReadByte();
                var listTag = new ListTag(name, listType);
                var tagCount = reader.ReadInt32();
                for (int i = 0; i < tagCount; i++)
                    listTag.Add(ReadTag(reader));
                return listTag;
            }
            else if (type == TagType.Compound) return ReadCompound(reader);
            else throw new Exception($"Unknown tag type: {type}");
        }

        #endregion

    }
}