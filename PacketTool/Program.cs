using System.Linq;
using System.Text;
using System.Text.Unicode;

namespace PacketTool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // 필요한 args : 
            // 1. 어떤 패킷들이 필요한지 정의한 파일 경로
            // 2. 패킷들을 정의한 출력 cs 파일을 추가할 파일 경로

            Console.WriteLine("Start Packet generation");

            if (args.Length != 2)
            {
                Console.WriteLine("패킷 자동 생성을 위해서는, def 파일 경로 및 출력 파일 경로가 필요합니다.");
                return;
            }

            string defPath = args[0];
            string outDir = args[1];

            if (!File.Exists(defPath))
                throw new FileNotFoundException(defPath);

            if (!Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);

            Build(defPath, outDir);
        }

        static void Build(string defPath, string outDir)
        {
            // 1. packets.def 파일 파싱
            List<PacketDef> packetDefs = File.ReadAllLines(defPath) // 전체 라인 읽음
                                        .Select(l => l.Trim()) // 각 라인
                                        .Where(l => l.Length > 0 && !l.StartsWith('#')) // 라인에 내용이 없거나 주석이면 제외
                                        .Select(Parse) // 유효한 라인들에 대해서 PacketDef DTO로 파싱
                                        .ToList();

            // 2. PacketId enum 타입 정의 .cs파일 저장
            string enumText = Build_PacketIdEnum(packetDefs);
            File.WriteAllText(Path.Combine(outDir, "PacketId.cs"), enumText, Encoding.UTF8);

            // 3. Packet class 전부 정의 .cs 파일 저장
            string classesText = Build_PacketClasses(packetDefs);
            File.WriteAllText(Path.Combine(outDir, "Packets.cs"), classesText, Encoding.UTF8);

            // 4. Packet Factory 정의 빌드 .cs 파일 저장
            string factoryClassText = Build_PacketFactoryClass(packetDefs);
            File.WriteAllText(Path.Combine(outDir, "PacketFactory.cs"), factoryClassText, Encoding.UTF8);
        }


        /// <summary>
        /// TODO : 로직 성능개선 필요함
        /// </summary>        
        static PacketDef Parse(string line)
        {
            string[] splits = line.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
            string idHex = splits[0];
            string name = splits[1];

            List<FieldDef> fields =
                splits[2].Split(',', StringSplitOptions.RemoveEmptyEntries)
                     .Select(l =>
                     {
                         string[] pair = l.Trim().Split(' ');
                         return new FieldDef(pair[1], pair[0]);
                     })
                     .ToList();
            return new PacketDef(name, idHex, fields);
        }

        static string Build_PacketIdEnum(IEnumerable<PacketDef> packetDefs)
        {
            StringBuilder sb = new StringBuilder("""
             namespace Game.Core.Network
             {
                 public enum PacketId : ushort
                 {
             """);
            sb.AppendLine();

            foreach (var packetDef in packetDefs)
            {
                sb.AppendLine($"        {packetDef.Name} = {packetDef.IdHex},");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        static string Build_PacketClasses(IEnumerable<PacketDef> packetDefs)
        {
            StringBuilder sb = new StringBuilder("""
             namespace Game.Core.Network
             {
             """);

            foreach (PacketDef packetDef in packetDefs)
            {
                sb.AppendLine(Build_PacketClass(packetDef));
            }

            sb.Append("}");

            return sb.ToString();
        }

        static string Build_PacketClass(PacketDef packetDef)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine();
            sb.AppendLine($"    public sealed class {packetDef.Name} : IPacket");
            sb.AppendLine("    {");
            sb.AppendLine($"        public PacketId PacketId => PacketId.{packetDef.Name};");
            sb.AppendLine();

            // fields
            // ------------------
            foreach(var fieldDef in packetDef.Fields)
            {
                sb.AppendLine($"        public {fieldDef.CsType} {fieldDef.Name} {{ get; set; }}");
            }
            sb.AppendLine();

            // Serialize()
            // ------------------
            sb.AppendLine("        public void Serialize(BinaryWriter writer)");
            sb.AppendLine("        {");

            foreach (var fieldDef in packetDef.Fields)
            {
                sb.AppendLine($"            writer.Write({fieldDef.Name});");
            }
            sb.AppendLine("        }");
            sb.AppendLine();

            // Deserialize()
            // ------------------
            sb.AppendLine("        public void Deserialize(BinaryReader reader)");
            sb.AppendLine("        {");
            foreach (var fieldDef in packetDef.Fields)
            {
                sb.AppendLine($"            {fieldDef.Name} = reader.Read{s_typeLookupTable[fieldDef.CsType].Name}();");
            }
            sb.AppendLine("        }");

            sb.Append("    }");
            return sb.ToString();
        }

        static string Build_PacketFactoryClass(IEnumerable<PacketDef> packetDefs)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("""
             namespace Game.Core.Network
             {
                 public static class PacketFactory
                 {
             """);
            sb.AppendLine();
            sb.AppendLine("        // 생성자 테이블");
            sb.AppendLine("        private static readonly Dictionary<PacketId, Func<IPacket>> _constructors = new()");
            sb.AppendLine("        {");

            foreach (var packetDef in packetDefs)
            {
                sb.AppendLine($"            {{ PacketId.{packetDef.Name}, () => new {packetDef.Name}() }},");
            }

            sb.AppendLine("        };");
            sb.AppendLine();

            sb.AppendLine("""
                        /// <summary>
                        /// 데이터 송신 시 원하는 패킷 객체를 Serialize 하기 위함
                        /// </summary>
                        /// <param name="packet"> 송신 대상 </param>
                        /// <returns> Serialized </returns>
                        public static byte[] ToBytes(IPacket packet)
                        {
                            using (MemoryStream stream = new MemoryStream())
                            using (BinaryWriter writer = new BinaryWriter(stream))
                            {
                                writer.Write((ushort)packet.PacketId);
                                packet.Serialize(writer);

                                return stream.ToArray();
                            }
                        }
                """);
            
            sb.AppendLine();
            sb.AppendLine("""
                        /// <summary>
                        /// 수신한 데이터를 Deserialize 하기 위함
                        /// </summary>
                        /// <param name="body"> 수신한 데이터 </param>
                        /// <returns> Deserialized </returns>
                        public static IPacket FromBytes(byte[] body)
                        {
                            // Packet Id 도 못 읽으면 잘못된 데이터임
                            if (body.Length < sizeof(PacketId))
                                return null;

                            PacketId packetId = (PacketId)BitConverter.ToUInt16(body, 0);

                            // 유효한 Packet Id 인지 
                            if (_constructors.TryGetValue(packetId, out Func<IPacket> constructor) == false)
                                return null;

                            IPacket packet = constructor.Invoke();

                            // packetId 만큼을 제외한 순수 데이터만 스트림에 취급
                            using (MemoryStream stream = new MemoryStream(body, sizeof(ushort), body.Length - sizeof(ushort)))
                            using (BinaryReader reader = new BinaryReader(stream))
                            {
                                packet.Deserialize(reader);
                            }

                            return packet;
                        }
                """);


            sb.AppendLine("    }");
            sb.Append("}");
            return sb.ToString();
        }

        record FieldDef(string Name, string CsType);
        record PacketDef(string Name, string IdHex, List<FieldDef> Fields);

        private readonly static Dictionary<string, Type> s_typeLookupTable = new()
        {
         // 정수 타입
         ["byte"] = typeof(byte),           // System.Byte (0 to 255)
         ["sbyte"] = typeof(sbyte),         // System.SByte (-128 to 127)
         ["short"] = typeof(short),         // System.Int16 (-32,768 to 32,767)
         ["ushort"] = typeof(ushort),       // System.UInt16 (0 to 65,535)
         ["int"] = typeof(int),             // System.Int32 (-2,147,483,648 to 2,147,483,647)
         ["uint"] = typeof(uint),           // System.UInt32 (0 to 4,294,967,295)
         ["long"] = typeof(long),           // System.Int64 (-9,223,372,036,854,775,808 to 9,223,372,036,854,775,807)
         ["ulong"] = typeof(ulong),         // System.UInt64 (0 to 18,446,744,073,709,551,615)

         // 부동소수점 타입
         ["float"] = typeof(float),         // System.Single (7 digits precision)
         ["double"] = typeof(double),       // System.Double (15-17 digits precision)
         ["decimal"] = typeof(decimal),     // System.Decimal (28-29 digits precision)

         // 기타 기본 타입
         ["bool"] = typeof(bool),           // System.Boolean (true/false)
         ["char"] = typeof(char),           // System.Char (16-bit Unicode character)
         ["string"] = typeof(string),       // System.String (sequence of characters)
         ["object"] = typeof(object),       // System.Object (base class of all types)

         // 특수 타입
         ["void"] = typeof(void),           // System.Void (메서드 반환 타입용)

         // nullable 변형들 (선택적 - 필요에 따라 추가)
         ["int?"] = typeof(int?),           // System.Nullable<System.Int32>
         ["bool?"] = typeof(bool?),         // System.Nullable<System.Boolean>
         ["byte?"] = typeof(byte?),         // System.Nullable<System.Byte>
         ["sbyte?"] = typeof(sbyte?),       // System.Nullable<System.SByte>
         ["short?"] = typeof(short?),       // System.Nullable<System.Int16>
         ["ushort?"] = typeof(ushort?),     // System.Nullable<System.UInt16>
         ["uint?"] = typeof(uint?),         // System.Nullable<System.UInt32>
         ["long?"] = typeof(long?),         // System.Nullable<System.Int64>
         ["ulong?"] = typeof(ulong?),       // System.Nullable<System.UInt64>
         ["float?"] = typeof(float?),       // System.Nullable<System.Single>
         ["double?"] = typeof(double?),     // System.Nullable<System.Double>
         ["decimal?"] = typeof(decimal?),   // System.Nullable<System.Decimal>
         ["char?"] = typeof(char?),         // System.Nullable<System.Char>

         // 일반적인 시스템 타입들 (별칭은 아니지만 자주 사용됨)
         ["DateTime"] = typeof(DateTime),           // System.DateTime
         ["TimeSpan"] = typeof(TimeSpan),           // System.TimeSpan
         ["Guid"] = typeof(Guid),                   // System.Guid
         ["IntPtr"] = typeof(IntPtr),               // System.IntPtr
         ["UIntPtr"] = typeof(UIntPtr),             // System.UIntPtr

         // 배열 타입들 (기본적인 것들)
         ["byte[]"] = typeof(byte[]),
         ["int[]"] = typeof(int[]),
         ["string[]"] = typeof(string[]),
         ["bool[]"] = typeof(bool[]),
         ["char[]"] = typeof(char[]),
         ["float[]"] = typeof(float[]),
         ["double[]"] = typeof(double[]),
         ["long[]"] = typeof(long[]),
         ["short[]"] = typeof(short[]),
         ["object[]"] = typeof(object[]),
        };
    }
}