// Copyright (C) 2019 Tricky Fast Studios, LLC
using System;
using System.Collections.Generic;
using Mirror;
using TrickyFast.CAT;
using TrickyFast.CAT.Values;
using UnityEngine;

namespace TrickyFast.Multiplayer
{
    public partial class MirrorMultiplayerEntity : NetworkBehaviour, IMultiplayerEntity
    {
        protected Dictionary<Guid, Deferred> pendingCalls = new Dictionary<Guid, Deferred>();

        [SyncVar]
        public string currentState;

        public void ChangeState(string state, CATContext context)
        {
            RpcChangeState(state);
        }

        [ClientRpc]
        public void RpcChangeState(string state)
        {
            var sm = GetComponent<StateMachine>();
            if (sm != null)
                sm.ChangeState(state, new CATContext(sm.gameObject));
            currentState = state;
        }

        //TODO: make sure it works on both directions
        public void RunAction(string path, CATContext context)
        {
            RpcRunAction(path);
        }

        [ClientRpc]
        public void RpcRunAction(string path)
        {
            var tr = transform.Find(path);
            if (tr == null) return;
            var action = tr.GetComponent<CATAction>();
            action.Run(new CATContext(action.GetComponentInParent<StateMachine>().gameObject));
        }

        public override void OnStartLocalPlayer()
        {
            if (isLocalPlayer)
            {
                var cond = Conductor.GetConductor();
                if (cond != null)
                {
                    var svc = cond.GetLocalServiceByInterface<IMultiplayerService>();
                    if (svc != null)
                    {
                        svc.LocalPlayer = gameObject;
                        Debug.Log("Setting local player", gameObject);
                    }
                }
            }
        }

        public virtual void SendEvent<T>(string eventName, T value)
        {
            if (IsServer && !IsClient)
            {
                if (value.GetType() == typeof(string))
                    RpcSendEventString(eventName, value.ToString());
                else if (value.GetType() == typeof(int))
                    RpcSendEventInteger(eventName, Convert.ToInt32(value));
                else if (value.GetType() == typeof(bool))
                    RpcSendEventBool(eventName, Convert.ToBoolean(value));
                else if (value.GetType() == typeof(float))
                    RpcSendEventFloat(eventName, Convert.ToSingle(value));
                else
                    Debug.LogError("Type " + value.GetType() + " Not supported for SendEvent!");
            }
            else
            {
                if (value.GetType() == typeof(string))
                    CmdSendServerEventString(eventName, value.ToString());
                else if (value.GetType() == typeof(int))
                    CmdSendServerEventInteger(eventName, Convert.ToInt32(value));
                else if (value.GetType() == typeof(bool))
                    CmdSendServerEventBool(eventName, Convert.ToBoolean(value));
                else if (value.GetType() == typeof(float))
                    CmdSendServerEventFloat(eventName, Convert.ToSingle(value));
                else
                    Debug.LogError("Type " + value.GetType() + " Not supported for SendEvent!");
            }
        }

        [ClientRpc]
        private void RpcSendEventString(string eventName, string value)
        {
            CATEventManager sm = GetComponent<StateMachine>();
            if (sm == null) sm = GetComponent<CATEventManager>();
            sm.FireEvent(eventName, value);
        }

        [ClientRpc]
        private void RpcSendEventInteger(string eventName, int value)
        {
            CATEventManager sm = GetComponent<StateMachine>();
            if (sm == null) sm = GetComponent<CATEventManager>();
            sm.FireEvent(eventName, value);
        }

        [ClientRpc]
        private void RpcSendEventBool(string eventName, bool value)
        {
            CATEventManager sm = GetComponent<StateMachine>();
            if (sm == null) sm = GetComponent<CATEventManager>();
            sm.FireEvent(eventName, value);
        }

        [ClientRpc]
        private void RpcSendEventFloat(string eventName, float value)
        {
            CATEventManager sm = GetComponent<StateMachine>();
            if (sm == null) sm = GetComponent<CATEventManager>();
            sm.FireEvent(eventName, value);
        }

        [Command]
        private void CmdSendServerEventString(string eventName, string value)
        {
            CATEventManager sm = GetComponent<StateMachine>();
            if (sm == null) sm = GetComponent<CATEventManager>();
            sm.FireEvent(eventName, value);
        }

        [Command]
        private void CmdSendServerEventInteger(string eventName, int value)
        {
            CATEventManager sm = GetComponent<StateMachine>();
            if (sm == null) sm = GetComponent<CATEventManager>();
            sm.FireEvent(eventName, value);
        }

        [Command]
        private void CmdSendServerEventBool(string eventName, bool value)
        {
            CATEventManager sm = GetComponent<StateMachine>();
            if (sm == null) sm = GetComponent<CATEventManager>();
            sm.FireEvent(eventName, value);
        }

        [Command]
        private void CmdSendServerEventFloat(string eventName, float value)
        {
            CATEventManager sm = GetComponent<StateMachine>();
            if (sm == null) sm = GetComponent<CATEventManager>();
            sm.FireEvent(eventName, value);
        }

        public void SetValue<T>(string namespaceName, T value)
        {
            if (IsServer)
            {
                if (value.GetType() == typeof(string))
                    RpcSendValueString(namespaceName, value.ToString());
                else if (value.GetType() == typeof(int))
                    RpcSendValueInteger(namespaceName, Convert.ToInt32(value));
                else if (value.GetType() == typeof(bool))
                    RpcSendValueBool(namespaceName, Convert.ToBoolean(value));
                else if (value.GetType() == typeof(float))
                    RpcSendValueFloat(namespaceName, Convert.ToSingle(value));
                else
                    Debug.LogError("Type " + value.GetType() + " Not supported for SendValue!");
            }
            else
            {
                if (value.GetType() == typeof(string))
                    CmdSendServerValueString(namespaceName, value.ToString());
                else if (value.GetType() == typeof(int))
                    CmdSendServerValueInteger(namespaceName, Convert.ToInt32(value));
                else if (value.GetType() == typeof(bool))
                    CmdSendServerValueBool(namespaceName, Convert.ToBoolean(value));
                else if (value.GetType() == typeof(float))
                    CmdSendServerValueFloat(namespaceName, Convert.ToSingle(value));
                else
                    Debug.LogError("Type " + value.GetType() + " Not supported for SendEvent!");
            }
        }

        [ClientRpc]
        private void RpcSendValueString(string eventName, string value)
        {
            var vh = GetComponent<ValueHolder>();
            if (vh.ContainsKey(eventName))
            {
                var val = vh[eventName] as CATGenericValueComponent<string>;
                if (val != null)
                {
                    val.IsReplicating = true;
                    val.SetValue(new CATContext(vh.gameObject), value);
                    val.IsReplicating = false;
                }
            }
            else
            {
                vh[eventName] = new StringValue(value);
            }
        }

        [ClientRpc]
        private void RpcSendValueInteger(string eventName, int value)
        {
            var vh = GetComponent<ValueHolder>();
            if (vh.ContainsKey(eventName))
            {
                var val = vh[eventName] as CATGenericValueComponent<int>;
                if (val != null)
                {
                    val.IsReplicating = true;
                    val.SetValue(new CATContext(vh.gameObject), value);
                    val.IsReplicating = false;
                }
            }
            else
            {
                vh[eventName] = new IntegerValue(value);
            }
        }

        [ClientRpc]
        private void RpcSendValueBool(string eventName, bool value)
        {
            var vh = GetComponent<ValueHolder>();
            if (vh.ContainsKey(eventName))
            {
                var val = vh[eventName] as CATGenericValueComponent<bool>;
                if (val != null)
                {
                    val.IsReplicating = true;
                    val.SetValue(new CATContext(vh.gameObject), value);
                    val.IsReplicating = false;
                }
            }
            else
            {
                vh[eventName] = new BoolValue(value);
            }
        }

        [ClientRpc]
        private void RpcSendValueFloat(string eventName, float value)
        {
            var vh = GetComponent<ValueHolder>();
            if (vh.ContainsKey(eventName))
            {
                var val = vh[eventName] as CATGenericValueComponent<float>;
                if (val != null)
                {
                    val.IsReplicating = true;
                    val.SetValue(new CATContext(vh.gameObject), value);
                    val.IsReplicating = false;
                }
            }
            else
            {
                vh[eventName] = new FloatValue(value);
            }
        }

        [Command]
        private void CmdSendServerValueString(string eventName, string value)
        {
            var vh = GetComponent<ValueHolder>();
            if (vh.ContainsKey(eventName))
            {
                var val = vh[eventName] as CATGenericValueComponent<string>;
                if (val != null)
                {
                    val.IsReplicating = true;
                    val.SetValue(new CATContext(vh.gameObject), value);
                    val.IsReplicating = false;
                }
            }
            else
            {
                vh[eventName] = new StringValue(value);
            }
        }

        [Command]
        private void CmdSendServerValueInteger(string eventName, int value)
        {
            var vh = GetComponent<ValueHolder>();
            if (vh.ContainsKey(eventName))
            {
                var val = vh[eventName] as CATGenericValueComponent<int>;
                if (val != null)
                {
                    val.IsReplicating = true;
                    val.SetValue(new CATContext(vh.gameObject), value);
                    val.IsReplicating = false;
                }
            }
            else
            {
                vh[eventName] = new IntegerValue(value);
            }
        }

        [Command]
        private void CmdSendServerValueBool(string eventName, bool value)
        {
            var vh = GetComponent<ValueHolder>();
            if (vh.ContainsKey(eventName))
            {
                var val = vh[eventName] as CATGenericValueComponent<bool>;
                if (val != null)
                {
                    val.IsReplicating = true;
                    val.SetValue(new CATContext(vh.gameObject), value);
                    val.IsReplicating = false;
                }
            }
            else
            {
                vh[eventName] = new BoolValue(value);
            }
        }

        [Command]
        private void CmdSendServerValueFloat(string eventName, float value)
        {
            var vh = GetComponent<ValueHolder>();
            if (vh.ContainsKey(eventName))
            {
                var val = vh[eventName] as CATGenericValueComponent<float>;
                if (val != null)
                {
                    val.IsReplicating = true;
                    val.SetValue(new CATContext(vh.gameObject), value);
                    val.IsReplicating = false;
                }
            }
            else
            {
                vh[eventName] = new FloatValue(value);
            }
        }

        protected virtual bool SendReturnValue(Guid guid, object value)
        {
            if (value.GetType() == typeof(bool))
            {
                if (IsServer)
                    RpcReturnValueBool(guid, (bool) value);
                else
                    CmdReturnValueBool(guid, (bool) value);
                return true;
            }

            if (value.GetType() == typeof(string))
            {
                if (IsServer)
                    RpcReturnValueString(guid, (string) value);
                else
                    CmdReturnValueString(guid, (string) value);
                return true;
            }

            if (value.GetType() == typeof(int))
            {
                if (IsServer)
                    RpcReturnValueInt(guid, (int) value);
                else
                    CmdReturnValueInt(guid, (int) value);
                return true;
            }

            if (value.GetType() == typeof(float))
            {
                if (IsServer)
                    RpcReturnValueFloat(guid, (float) value);
                else
                    CmdReturnValueFloat(guid, (float) value);
                return true;
            }

            if (value.GetType() == typeof(GameObject))
            {
                if (IsServer)
                    RpcReturnValueGameObject(guid, (GameObject) value);
                else
                    CmdReturnValueGameObject(guid, (GameObject) value);
                return true;
            }

            if (value.GetType().IsSubclassOf(typeof(Exception)) || value.GetType() == typeof(Exception))
            {
                if (IsServer)
                    RpcReturnValueError(guid, value.ToString());
                else
                    CmdReturnValueError(guid, value.ToString());
                return true;
            }

            return false;
        }

        [Command]
        private void CmdReturnValueBool(Guid guid, bool value)
        {
            HandleRemoteCallResult(guid, value);
        }

        [Command]
        private void CmdReturnValueString(Guid guid, string value)
        {
            HandleRemoteCallResult(guid, value);
        }

        [Command]
        private void CmdReturnValueFloat(Guid guid, float value)
        {
            HandleRemoteCallResult(guid, value);
        }

        [Command]
        private void CmdReturnValueInt(Guid guid, int value)
        {
            HandleRemoteCallResult(guid, value);
        }

        [Command]
        private void CmdReturnValueGameObject(Guid guid, GameObject value)
        {
            HandleRemoteCallResult(guid, value);
        }

        [Command]
        private void CmdReturnValueError(Guid guid, string message)
        {
            HandleRemoteCallResult(guid, new Exception(message));
        }

        [ClientRpc]
        private void RpcReturnValueBool(Guid guid, bool value)
        {
            HandleRemoteCallResult(guid, value);
        }

        [ClientRpc]
        private void RpcReturnValueString(Guid guid, string value)
        {
            HandleRemoteCallResult(guid, value);
        }

        [ClientRpc]
        private void RpcReturnValueFloat(Guid guid, float value)
        {
            HandleRemoteCallResult(guid, value);
        }

        [ClientRpc]
        private void RpcReturnValueInt(Guid guid, int value)
        {
            HandleRemoteCallResult(guid, value);
        }

        [ClientRpc]
        private void RpcReturnValueGameObject(Guid guid, GameObject value)
        {
            HandleRemoteCallResult(guid, value);
        }

        [ClientRpc]
        private void RpcReturnValueError(Guid guid, string message)
        {
            HandleRemoteCallResult(guid, new Exception(message));
        }

        protected Deferred CallRemote(Action<Guid> call)
        {
            var guid = Guid.NewGuid();
            var dfrd = new Deferred();
            pendingCalls[guid] = dfrd;
            call(guid);
            return dfrd;
        }

        protected void HandleRemoteCallResult(Guid guid, object result)
        {
            if (pendingCalls.ContainsKey(guid))
            {
                pendingCalls[guid].Callback(result);
                pendingCalls.Remove(guid);
            }
        }

        public bool IsLocalPlayer
        {
            get { return isLocalPlayer; }
        }

        public bool IsServer
        {
            get { return isServer; }
        }

        public bool IsClient
        {
            get { return isClient; }
        }

        public string CurrentState
        {
            get { return currentState; }
        }
    }
}