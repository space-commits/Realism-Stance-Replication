using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Comfort.Common;
using EFT.UI;
using Fika.Core.Coop.Components;
using Fika.Core.Coop.Matchmaker;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StanceReplication
{
    [BepInPlugin("com.lacyway.rsr", "RealismStanceReplication", "1.0.0")]
    [BepInDependency("com.fika.core", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource REAL_Logger;
        public static Dictionary<int, RSR_Observed_Component> ObservedComponents;
        public static ConfigEntry<bool> EnableForBots { get; set; }
        public static ConfigEntry<float> CancelTimer { get; set; }
        public static ConfigEntry<float> ResetTimer { get; set; }

        public static ConfigEntry<float> Test1 { get; set; }
        public static ConfigEntry<float> Test2 { get; set; }
        public static ConfigEntry<float> Test3 { get; set; }

        protected void Awake()
        {
            EnableForBots = Config.Bind<bool>("Options", "Enable Stance Replication For Bots", true, new ConfigDescription("Requires Restart. Toggles replication for bots. Disabling can help improve performance if there are any issues.", null, new ConfigurationManagerAttributes { Order = 1 }));
            ResetTimer = Config.Bind<float>("Options", "Reset Timer", 0.2f, new ConfigDescription("Time before stance resets after sprinting or collision.", new AcceptableValueRange<float>(0.0f, 20f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 3, IsAdvanced = true }));
            CancelTimer = Config.Bind<float>("Options", "Cancel Timer", 0.2f, new ConfigDescription("Time before stance is cancelled due to sprinting or collision.", new AcceptableValueRange<float>(0.0f, 20f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 4, IsAdvanced = true }));

            Test1 = Config.Bind<float>("Debug", "Test Value 1", 1f, new ConfigDescription("", new AcceptableValueRange<float>(-1000f, 1000f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 3, IsAdvanced = true }));
            Test2 = Config.Bind<float>("Debug", "Test Value 2", 1f, new ConfigDescription("", new AcceptableValueRange<float>(-1000f, 1000f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 2, IsAdvanced = true }));
            Test3 = Config.Bind<float>("Debug", "Test Value 3", 1f, new ConfigDescription("", new AcceptableValueRange<float>(-1000f, 1000f), new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 1, IsAdvanced = true }));



            new CoopBot_Create_Patch().Enable();
            new CoopPlayer_Create_Patch().Enable();
            new ObservedCoopPlayer_Create_Patch().Enable();

            FikaEventDispatcher.SubscribeEvent<FikaClientCreatedEvent>(ClientCreated);
            FikaEventDispatcher.SubscribeEvent<FikaServerCreatedEvent>(ServerCreated);
            FikaEventDispatcher.SubscribeEvent<FikaGameCreatedEvent>(GameWorldStarted);

            ObservedComponents = new Dictionary<int, RSR_Observed_Component>();

            REAL_Logger = Logger;
            REAL_Logger.LogInfo($"{nameof(Plugin)} has been loaded.");

        }

        private void GameWorldStarted(FikaGameCreatedEvent @event)
        {
            if (ObservedComponents != null)
            {
                ObservedComponents.Clear();
            }
            else
            {
                ObservedComponents = new Dictionary<int, RSR_Observed_Component>();
            }
        }

        private void ServerCreated(FikaServerCreatedEvent @event)
        {
            @event.Server.packetProcessor.SubscribeNetSerializable<RealismPacket, NetPeer>(HandleRealismPacketServer);
        }

        private void ClientCreated(FikaClientCreatedEvent @event)
        {
            @event.Client.packetProcessor.SubscribeNetSerializable<RealismPacket>(HandleRealismPacketClient);
        }

        private void HandleRealismPacketClient(RealismPacket packet)
        {
            if (ObservedComponents.TryGetValue(packet.NetID, out var player))
            {
                player.SetAnimValues(packet.WeapPosition, packet.Rotation, packet.IsPatrol, packet.SprintAnimationVarient);
            }
        }

        private void HandleRealismPacketServer(RealismPacket packet, NetPeer peer)
        {
            if (ObservedComponents.TryGetValue(packet.NetID, out var player))
            {
                player.SetAnimValues(packet.WeapPosition, packet.Rotation, packet.IsPatrol, packet.SprintAnimationVarient);
            }
            Singleton<FikaServer>.Instance.SendDataToAll(new NetDataWriter(), ref packet, DeliveryMethod.Unreliable, peer);
        }
    }
}
