using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace ClimbGames
{
    public interface ITable
    {
        void Initialize();
    }

    public abstract class Table : ScriptableObject, ITable
    {
        public abstract IReadOnlyList<TableRecord> GetDatas();
        public abstract void Initialize();
        public abstract byte[] ToBytes();
        public abstract void Load(byte[] bytes);
    }

    public abstract class Table<TRecord> : Table where TRecord : TableRecord, new()
    {
        [SerializeField] protected List<TRecord> datas;
        public IReadOnlyList<TRecord> Datas => datas;

        public override IReadOnlyList<TableRecord> GetDatas() => datas;
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

        public override void Load(byte[] bytes)
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
        }
    }

    public static class Tables<T> where T : Table
    {
        public static T FromBytes(byte[] bytes)
        {
            Table table = ScriptableObject.CreateInstance<T>();
            table.Load(bytes);
            return table as T;
        }
    }
}