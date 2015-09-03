﻿using FSO.Server.Protocol.Utils;
using FSO.Server.Protocol.Voltron.Model;
using Mina.Core.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSO.Server.Protocol.Voltron
{
    public abstract class AbstractVoltronPacket : IVoltronPacket
    {
        public static Sender GetSender(IoBuffer buffer)
        {
            var ariesID = buffer.GetPascalString();
            var masterID = buffer.GetPascalString();
            return new Sender { AriesID = ariesID, MasterAccountID = masterID };
        }

        public static void PutSender(IoBuffer buffer, Sender sender)
        {
            buffer.PutPascalString(sender.AriesID);
            buffer.PutPascalString(sender.MasterAccountID);
        }

        public static IoBuffer Allocate(int size)
        {
            IoBuffer buffer = IoBuffer.Allocate(size);
            buffer.Order = ByteOrder.BigEndian;
            return buffer;
        }

        public abstract VoltronPacketType GetPacketType();
        public abstract IoBuffer Serialize();
        public abstract void Deserialize(IoBuffer input);
    }
}