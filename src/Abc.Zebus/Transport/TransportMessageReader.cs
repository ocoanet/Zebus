using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Abc.Zebus.Serialization.Protobuf;
using ProtoBuf;
using Buffer = Abc.Zebus.Util.Buffer;

namespace Abc.Zebus.Transport
{
    internal static class TransportMessageReader
    {
        // This class is used from a single thread
        private static readonly Dictionary<Buffer, string> _strings = new Dictionary<Buffer, string>
        {
            { default, string.Empty }
        };

        internal static TransportMessage ReadTransportMessage(this CodedInputStream input)
        {
            var transportMessage = new TransportMessage { Content = Stream.Null };

            while (!input.IsAtEnd && input.TryReadTag(out var number, out var wireType))
            {
                switch (number)
                {
                    case 1:
                        transportMessage.Id = ReadMessageId(input);
                        break;
                    case 2:
                        transportMessage.MessageTypeId = ReadMessageTypeId(input);
                        break;
                    case 3:
                        transportMessage.Content = ReadStream(input);
                        break;
                    case 4:
                        transportMessage.Originator = ReadOriginatorInfo(input);
                        break;
                    case 5:
                        transportMessage.Environment = ReadCachedString(input);
                        break;
                    case 6:
                        transportMessage.WasPersisted = input.ReadBool();
                        break;
                    case 7:
                        transportMessage.PersistentPeerIds = ReadPeerIds(input, transportMessage.PersistentPeerIds);
                        break;
                    default:
                        SkipUnknown(input, wireType);
                        break;
                }
            }

            return transportMessage;
        }

        private static OriginatorInfo ReadOriginatorInfo(CodedInputStream input)
        {
            var length = input.ReadLength();
            var endPosition = input.Position + length;

            var senderId = new PeerId();
            string senderEndPoint = null;
            string initiatorUserName = null;

            while (input.Position < endPosition && input.TryReadTag(out var number, out var wireType))
            {
                switch (number)
                {
                    case 1:
                        senderId = ReadPeerId(input);
                        break;
                    case 2:
                        senderEndPoint = ReadCachedString(input);
                        break;
                    case 5:
                        initiatorUserName = ReadCachedString(input);
                        break;
                    default:
                        SkipUnknown(input, wireType);
                        break;
                }
            }

            return new OriginatorInfo(senderId, senderEndPoint, null, initiatorUserName);
        }

        private static PeerId ReadPeerId(CodedInputStream input)
        {
            var value = ReadSingleFieldString(input);
            return new PeerId(value);
        }

        private static MessageId ReadMessageId(CodedInputStream input)
        {
            var guid = ReadSingleFieldGuid(input);
            return new MessageId(guid);
        }

        private static MessageTypeId ReadMessageTypeId(CodedInputStream input)
        {
            var fullName = ReadSingleFieldCachedString(input);
            return new MessageTypeId(fullName);
        }

        private static Stream ReadStream(CodedInputStream input)
        {
            var length = input.ReadLength();
            return new MemoryStream(input.ReadRawBytes(length));
        }

        private static string ReadCachedString(CodedInputStream input)
            => GetCachedString(input.ReadBuffer());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetCachedString(in Buffer buffer)
        {
            if (_strings.TryGetValue(buffer, out var result))
                return result;

            return AddCachedString(in buffer);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string AddCachedString(in Buffer buffer)
        {
            var result = CodedOutputStream.Utf8Encoding.GetString(buffer.Data, buffer.Offset, buffer.Length);
            _strings[buffer.Copy()] = result;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string ReadSingleFieldString(CodedInputStream input)
        {
            var length = input.ReadLength();
            var endPosition = input.Position + length;

            string value = null;

            while (input.Position < endPosition && input.TryReadTag(out var number, out var wireType))
            {
                switch (number)
                {
                    case 1:
                        value = input.ReadString();
                        break;
                    default:
                        SkipUnknown(input, wireType);
                        break;
                }
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string ReadSingleFieldCachedString(CodedInputStream input)
        {
            var length = input.ReadLength();
            var endPosition = input.Position + length;

            string value = null;

            while (input.Position < endPosition && input.TryReadTag(out var number, out var wireType))
            {
                switch (number)
                {
                    case 1:
                        value = ReadCachedString(input);
                        break;
                    default:
                        SkipUnknown(input, wireType);
                        break;
                }
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Guid ReadSingleFieldGuid(CodedInputStream input)
        {
            var length = input.ReadLength();
            var endPosition = input.Position + length;

            Guid value = default;

            while (input.Position < endPosition && input.TryReadTag(out var number, out var wireType))
            {
                switch (number)
                {
                    case 1:
                        value = input.ReadGuid();
                        break;
                    default:
                        SkipUnknown(input, wireType);
                        break;
                }
            }

            return value;
        }

        private static List<PeerId> ReadPeerIds(CodedInputStream input, List<PeerId> peerIds)
        {
            if (peerIds == null)
                peerIds = new List<PeerId>();

            var value = ReadSingleFieldCachedString(input);
            peerIds.Add(new PeerId(value));

            return peerIds;
        }

        private static void SkipUnknown(CodedInputStream input, WireType wireType)
        {
            switch (wireType)
            {
                case WireType.None:
                    break;
                case WireType.Variant:
                    input.ReadRawVarint32();
                    break;
                case WireType.Fixed64:
                    input.ReadFixed64();
                    break;
                case WireType.String:
                    input.SkipString();
                    break;
                case WireType.StartGroup:
                    break;
                case WireType.EndGroup:
                    break;
                case WireType.Fixed32:
                    input.ReadFixed32();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
