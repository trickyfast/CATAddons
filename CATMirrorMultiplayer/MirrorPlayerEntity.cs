// Copyright (C) 2019 Tricky Fast Studios, LLC
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using TrickyFast.Player;
using UnityEngine;

namespace TrickyFast.Multiplayer
{
    [RequireComponent(typeof(PlayerCharacter))]
    public class MirrorPlayerEntity : MirrorMultiplayerEntity, IMultiplayerPlayerEntity
    {
        public IConnection Connection
        {
            get
            {
                var conductor = Conductor.GetConductor();
                var mps = conductor.GetLocalServiceByInterface<IMultiplayerService>();
                var mmps = mps as MirrorService;
                return mmps.FindConnection(connectionToClient);
            }
        }

        public Deferred CreateAccount(string username, string password, string email)
        {
            if (isServer)
            {
                var cond = Conductor.GetConductor();
                if (cond == null)
                {
                    Debug.LogError("A conductor is required to log in!", gameObject);
                    return Deferred.Fail(new Exception("A conductor is required to log in!"));
                }
                var ps = cond.GetLocalServiceByInterface<IPlayerService>();
                if (ps == null)
                {
                    Debug.LogError("A player service is required!", gameObject);
                    return Deferred.Fail(new Exception("A player service is required!"));
                }
                return ps.CreateAccount(username, password, email).AddCallback((obj, res) =>
                {
                    var pc = GetComponent<PlayerCharacter>();
                    pc.account = obj as Account;
                    return obj;
                });
            }
            return CallRemote(g => CmdCreateAccount(username, password, email, g)).AddCallback((res, p) =>
            {
                var pc = GetComponent<PlayerCharacter>();
                pc.account = res as Account;
                return res;
            });
        }

        [Command]
        private void CmdCreateAccount(string username, string password, string email, Guid guid)
        {
            CreateAccount(username, password, email).AddBoth((res, p) =>
            {
                SendReturnValue(guid, res);
                return res;
            });
        }

        public Deferred GetCharacters()
        {
            if (isServer)
            {
                var cond = Conductor.GetConductor();
                if (cond == null)
                {
                    Debug.LogError("A conductor is required to log in!", gameObject);
                    return Deferred.Fail(new Exception("A conductor is required to log in!"));
                }
                var ps = cond.GetLocalServiceByInterface<IPlayerService>();
                if (ps == null)
                {
                    Debug.LogError("A player service is required!", gameObject);
                    return Deferred.Fail(new Exception("A player service is required!"));
                }
                var pl = GetComponent<PlayerCharacter>();
                return ps.GetCharacters(pl.account);
            }
            return CallRemote(g => CmdGetCharacters(g));
        }

        [Command]
        private void CmdGetCharacters(Guid guid)
        {
            GetCharacters().AddBoth((res, p) =>
            {
                SendReturnValue(guid, res);
                return res;
            });
        }

        public Deferred LogIn(string username, string password)
        {
            if (isServer)
            {
                var cond = Conductor.GetConductor();
                if (cond == null)
                {
                    Debug.LogError("A conductor is required to log in!", gameObject);
                    return Deferred.Fail(new Exception("A conductor is required to log in!"));
                }
                var ps = cond.GetLocalServiceByInterface<IMultiplayerService>();
                if (ps == null)
                {
                    Debug.LogError("A player service is required!", gameObject);
                    return Deferred.Fail(new Exception("A player service is required!"));
                }
                return ps.LogIn(Connection, username, password).AddCallback((obj, res) =>
                {
                    var pc = GetComponent<PlayerCharacter>();
                    pc.account = obj as Account;
                    return obj;
                });
            }
            return CallRemote(g => CmdLogIn(username, password, g)).AddCallback((res, p) =>
            {
                var pc = GetComponent<PlayerCharacter>();
                pc.account = res as Account;
                name = "Player - " + pc.account.username;
                return res;
            });
        }

        [Command]
        private void CmdLogIn(string username, string password, Guid guid)
        {
            LogIn(username, password).AddBoth((res, p) =>
            {
                SendReturnValue(guid, res);
                return res;
            });
        }

        public Deferred SelectCharacter(Character character)
        {
            if (isServer)
            {
                var cond = Conductor.GetConductor();
                if (cond == null)
                {
                    Debug.LogError("A conductor is required to log in!", gameObject);
                    return Deferred.Fail(new Exception("A conductor is required to log in!"));
                }
                var ps = cond.GetLocalServiceByInterface<IPlayerService>();
                if (ps == null)
                {
                    Debug.LogError("A player service is required!", gameObject);
                    return Deferred.Fail(new Exception("A player service is required!"));
                }
                var pc = GetComponent<PlayerCharacter>();
                return ps.SelectCharacter(pc.account, character).AddCallback((obj, res) =>
                {
                    pc.character = obj as Character;
                    pc.name = "Player - " + pc.account.username + " - " + pc.character.name;
                    return obj;
                });
            }
            return CallRemote(g => CmdSelectCharacter(new SerializedCharacter(character), g)).AddCallback((res, p) =>
            {
                var pc = GetComponent<PlayerCharacter>();
                pc.character = res as Character;
                pc.name = "Player - " + pc.account.username + " - " + pc.character.name;
                return res;
            });
        }

        [Command]
        private void CmdSelectCharacter(SerializedCharacter character, Guid guid)
        {
            SelectCharacter(character.ToCharacter()).AddBoth((res, p) =>
            {
                SendReturnValue(guid, res);
                return res;
            });
        }

        protected override bool SendReturnValue(Guid guid, object value)
        {
            if (base.SendReturnValue(guid, value)) return true;
            if (value.GetType() == typeof(Character))
            {
                if (IsServer)
                    RpcReturnValueCharacter(guid, new SerializedCharacter((Character)value));
                else
                    CmdReturnValueCharacter(guid, new SerializedCharacter((Character)value));
                return true;
            }
            if (value.GetType() == typeof(List<Character>))
            {
                List<Character> vlist = value as List<Character>;
                SerializedCharacter[] clist = new SerializedCharacter[vlist.Count];
                for (int index = 0; index < vlist.Count; ++index)
                    clist[index] = new SerializedCharacter(vlist[index]);
                if (IsServer)
                    RpcReturnValueCharacterList(guid, clist);
                else
                    CmdReturnValueCharacterList(guid, clist);
                return true;
            }
            if (value.GetType() == typeof(Account))
            {
                if (IsServer)
                    RpcReturnValueAccount(guid, (Account)value);
                else
                    CmdReturnValueAccount(guid, (Account)value);
                return true;
            }
            return false;
        }

        [Command]
        private void CmdReturnValueAccount(Guid guid, Account value)
        {
            HandleRemoteCallResult(guid, value);
        }

        [ClientRpc]
        private void RpcReturnValueAccount(Guid guid, Account value)
        {
            HandleRemoteCallResult(guid, value);
        }

        [Command]
        private void CmdReturnValueCharacter(Guid guid, SerializedCharacter value)
        {
            HandleRemoteCallResult(guid, value.ToCharacter());
        }

        [ClientRpc]
        private void RpcReturnValueCharacter(Guid guid, SerializedCharacter value)
        {
            HandleRemoteCallResult(guid, value.ToCharacter());
        }

        [Command]
        private void CmdReturnValueCharacterList(Guid guid, SerializedCharacter[] value)
        {
            List<Character> clist = new List<Character>();
            for (int index = 0; index < value.Length; ++index)
                clist.Add(value[index].ToCharacter());
            HandleRemoteCallResult(guid, clist);
        }

        [ClientRpc]
        private void RpcReturnValueCharacterList(Guid guid, SerializedCharacter[] value)
        {
            List<Character> clist = new List<Character>();
            for (int index = 0; index < value.Length; ++index)
                clist.Add(value[index].ToCharacter());
            HandleRemoteCallResult(guid, clist);
        }

        public Deferred CreateCharacter(string name)
        {
            if (isServer)
            {
                var cond = Conductor.GetConductor();
                if (cond == null)
                {
                    Debug.LogError("A conductor is required to log in!", gameObject);
                    return Deferred.Fail(new Exception("A conductor is required to log in!"));
                }
                var ps = cond.GetLocalServiceByInterface<IPlayerService>();
                if (ps == null)
                {
                    Debug.LogError("A player service is required!", gameObject);
                    return Deferred.Fail(new Exception("A player service is required!"));
                }
                var conn = Connection as ConnectedUser;
                return ps.CreateCharacter(conn.account, name);
            }
            return CallRemote(g => CmdCreateCharacter(name, g));
        }

        [Command]
        private void CmdCreateCharacter(string name, Guid guid)
        {
            CreateCharacter(name).AddBoth((res, p) =>
            {
                SendReturnValue(guid, res);
                return res;
            });
        }

        public Deferred DeleteCharacter(Character character)
        {
            if (isServer)
            {
                var cond = Conductor.GetConductor();
                if (cond == null)
                {
                    Debug.LogError("A conductor is required to log in!", gameObject);
                    return Deferred.Fail(new Exception("A conductor is required to log in!"));
                }
                var ps = cond.GetLocalServiceByInterface<IPlayerService>();
                if (ps == null)
                {
                    Debug.LogError("A player service is required!", gameObject);
                    return Deferred.Fail(new Exception("A player service is required!"));
                }
                var conn = Connection as ConnectedUser;
                return ps.DeleteCharacter(conn.account, character);
            }
            return CallRemote(g => CmdDeleteCharacter(new SerializedCharacter(character), g));
        }

        [Command]
        private void CmdDeleteCharacter(SerializedCharacter character, Guid guid)
        {
            DeleteCharacter(character.ToCharacter()).AddBoth((res, p) =>
            {
                SendReturnValue(guid, res);
                return res;
            });
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            StartCoroutine(DelayedStartServer());
        }

        private IEnumerator DelayedStartServer()
        {
            yield return new WaitForEndOfFrame();
            var conductor = Conductor.GetConductor();
            var mps = conductor.GetLocalServiceByInterface<IMultiplayerService>();
            var mmps = mps as MirrorService;
            var conn = mmps.FindConnection(connectionToClient);
            conn.player = gameObject;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            if (isLocalPlayer)
            {
                var conductor = Conductor.GetConductor();
                var mps = conductor.GetLocalServiceByInterface<IMultiplayerService>();
                mps.LocalPlayer = gameObject;
                Debug.Log("Setting local player " + name, gameObject);
            }
        }
    }

    class SerializedCharacter
    {
        public string name;
        public string title;
        public string gender;
        public string description;
        public string characterRace;
        public string characterClass;

        public SerializedCharacter()
        {
        }

        public SerializedCharacter(Character character)
        {
            name = character.name;
            title = character.title;
            gender = character.gender;
            description = character.description;
            if (character.characterRace != null)
                characterRace = character.characterRace.name;
            if (character.characterRace != null)
                characterClass = character.characterClass.name;
        }

        public Character ToCharacter()
        {
            Character c = new Character();
            c.name = name;
            c.title = title;
            c.gender = gender;
            c.description = description;
            var cond = Conductor.GetConductor();
            if (cond != null)
            {
                var ps = cond.GetLocalServiceByInterface<IPlayerService>();
                if (ps != null)
                {
                    if (!string.IsNullOrEmpty(characterClass))
                        c.characterClass = ps.GetCharacterClass(characterClass);
                    if (!string.IsNullOrEmpty(characterRace))
                        c.characterRace = ps.GetCharacterRace(characterRace);
                }
            }
            return c;
        }
    }

    /*class SerializedCharacterList : List<SerializedCharacter>
    {
        public SerializedCharacterList() : base()
        {

        }

        public SerializedCharacterList(List<Character> characters) : base()
        {
            for (int index = 0; index < characters.Count; ++index)
                Add(new SerializedCharacter(characters[index]));
        }

        public List<Character> ToCharacterList()
        {
            var lst = new List<Character>();
            for (int index = 0; index < Count; ++index)
            {
                lst.Add(this[index].ToCharacter());
            }
            return lst;
        }
    }*/
}