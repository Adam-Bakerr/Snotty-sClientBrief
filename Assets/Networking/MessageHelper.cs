using Riptide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static NetworkManager;

namespace Assets
{
    static class MessageHelper
    {
        //A list of possible message types
        public enum messageTypes
        {
            Byte, Bool, Int, Uint, Float, String, Struct,
            ByteArray, BoolArray, IntArray, UintArray, FloatArray, StringArray, StructArray,

            //Custom Types
            ClientConnection, ClientList ,LobbyReady ,GameStart , PlayerTransformSync, StatSync,
            SpawnCollectables, CollectablePickedUp
        }


        public static Message CreateMessage(messageTypes type, MessageSendMode sendMode = MessageSendMode.Unreliable
        , [CallerMemberName] string callerName = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            Message message = Message.Create(sendMode, (ushort)type);
            return message;
        }

        public static void AddToMessage<T>(ref Message message, messageTypes type, T data, MessageSendMode sendMode = MessageSendMode.Unreliable
    , [CallerMemberName] string callerName = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            if (message == null) throw new Exception($"{callerName} Attempted To Add Data To NULL Message At Line {callerLineNumber}");

            switch (type)
            {
                case messageTypes.Byte:
                    message.AddByte((byte)(object)data);
                    break;
                case messageTypes.Bool:
                    message.AddBool((bool)(object)data);
                    break;
                case messageTypes.Int:
                    message.AddInt((int)(object)data);
                    break;
                case messageTypes.Uint:
                    message.AddUInt((uint)(object)data);
                    break;
                case messageTypes.Float:
                    message.AddFloat((float)(object)data);
                    break;
                case messageTypes.String:
                    message.AddString((string)(object)data);
                    break;
                case messageTypes.ByteArray:
                    message.AddBytes((byte[])(object)data);
                    break;
                case messageTypes.BoolArray:
                    message.AddBools((bool[])(object)data);
                    break;
                case messageTypes.IntArray:
                    message.AddInts((int[])(object)data);
                    break;
                case messageTypes.UintArray:
                    message.AddUInts((uint[])(object)data);
                    break;
                case messageTypes.FloatArray:
                    message.AddFloats((float[])(object)data);
                    break;
                case messageTypes.StringArray:
                    message.AddStrings((string[])(object)data);
                    break;
                case messageTypes.Struct:
                    message.AddSerializable((IMessageSerializable)(object)data);
                    break;
                case messageTypes.StructArray:
                    message.AddSerializables((IMessageSerializable[])(object)data);
                    break;
                default:
                    throw new Exception($"{callerName} Attempted To Add Invalid Message Type At Line {callerLineNumber}");
            }
        }
    }
}
