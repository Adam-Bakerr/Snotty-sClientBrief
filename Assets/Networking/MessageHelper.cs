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
            SpawnCollectables, CollectablePickedUp, GameFailed, GameCompleted, ExitZoneTriggered, SnottyPositionUpdate
           ,PlayerRevive, PlayerDowned
        }


        public static Message CreateMessage(messageTypes type, MessageSendMode sendMode = MessageSendMode.Unreliable
        , [CallerMemberName] string callerName = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            Message message = Message.Create(sendMode, (ushort)type);
            return message;
        }

    }
}
