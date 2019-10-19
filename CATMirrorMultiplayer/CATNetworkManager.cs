// Copyright (C) 2019 Tricky Fast Studios, LLC
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TrickyFast.Multiplayer
{
    public class CATNetworkManager : NetworkManager
    {
        MirrorService service;

        public override void Start()
        {
            base.Start();
            service = GetComponent<MirrorService>();
        }

        public override void OnServerConnect(NetworkConnection conn)
        {
            base.OnServerConnect(conn);
            service.OnClientConnected(conn);
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            base.OnServerDisconnect(conn);
            service.OnClientDisconnected(conn);
        }
    }
}