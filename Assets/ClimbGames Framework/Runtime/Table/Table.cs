using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace ClimbGames
{
    public interface ITable
    {
        void Initialize();
    }

    public abstract class Table : ScriptableObject, ITable
    {
        public abstract void Initialize();
        public abstract byte[] ToBytes();
    }

    public abstract class Table<TRecord> : Table where TRecord : TableRecord, new()
    {
        [SerializeField] protected List<TRecord> datas;

        protected virtual void OnInitialized() { }

        public override byte[] ToBytes()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms, System.Text.Encoding.UTF8, true))
                {
                    bw.Write("0.1.0");
                    bw.Write(datas.Count);

                    for (int i = 0; i < datas.Count; ++i)
                        datas[i].Write(bw);

                    bw.Write(0);
                }
                return ms.ToArray();
            }
        }

        public Table<TRecord> Load(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (BinaryReader br = new BinaryReader(ms, System.Text.Encoding.UTF8, true))
                {
                    br.ReadString();
                    int count = br.ReadInt32();

                    datas ??= new List<TRecord>(count);
                    datas.Clear();

                    for (int i = 0; i < count; ++i)
                    {
                        var record = new TRecord();
                        record.Read(br);
                        datas.Add(record);
                    }

                    br.ReadInt32();
                }
            }

            Initialize();
            return this;
        }
    }
}