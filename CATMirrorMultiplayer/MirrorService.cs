// Copyright (C) 2019 Tricky Fast Studios, LLC
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using TrickyFast.CAT;
using TrickyFast.Player;
using TrickyFast.Storage;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TrickyFast.Multiplayer
{
    [RequireComponent(typeof(CATNetworkManager), typeof(MirrorMultiplayerEntity))]
    [CATegory("Services")]
    public class MirrorService : ServiceBehaviour, IMultiplayerService, IPlayerService
    {
        public CharacterDefinition characterDefinition;
        public Account localAccount;
        public Character localCharacter;
        public List<CharacterClass> classes;
        public List<CharacterRace> races;

        public int PlayerCount => netMan.numPlayers;
        public bool IsServer => NetworkServer.active;
        public bool IsClient => NetworkClient.active;
        public bool IsConnected => NetworkClient.isConnected;
        public GameObject LocalPlayer { get; set; }

        public List<IConnection> Connections
        {
            get
            {
                var conns = new List<IConnection>();
                conns.AddRange(connections);
                return conns;
            }
        }

        private CATNetworkManager netMan;
        private MirrorMultiplayerEntity entity;
        private List<ConnectedUser> connections;

        public void Initialize(Conductor conductor)
        {
            netMan = GetComponent<CATNetworkManager>();
            entity = GetComponent<MirrorMultiplayerEntity>();
            connections = new List<ConnectedUser>();
        }

        public void StartClient(string host, int port, string username, string password)
        {
            netMan.networkAddress = host;
            netMan.StartClient();
        }

        public void StartServer(int port)
        {
            netMan.StartHost();
        }

        public Deferred Stop()
        {
            return Deferred.Succeed(true);
        }

        public void Spawn(GameObject obj)
        {
            if (obj.GetComponent<NetworkIdentity>() != null)
            {
                NetworkServer.Spawn(obj);
            }
        }

        public void Destroy(GameObject obj)
        {
            if (obj.GetComponent<NetworkIdentity>() != null)
                NetworkServer.Destroy(obj);
        }

        public void ChangeScene(string sceneName, LoadSceneMode sceneMode, LocalPhysicsMode physicsMode)
        {
            netMan.ServerChangeScene(sceneName, sceneMode, physicsMode);
        }

        public void SendEvent<T>(string eventName, T value)
        {
            entity.SendEvent(eventName, value);
        }

        public string GetAccountID(GameObject owner)
        {
            if (owner != null)
            {
                var pl = owner.GetComponent<PlayerCharacter>();
                if (pl != null && pl.account != null)
                    return pl.account.username;
            }
            return null;
        }

        public string GetCharacterID(Account account)
        {
            if (account == null)
            {
                Debug.LogWarning("Account passed in to GetCharacterID was null!");
                return null;
            }
            for (int index = 0; index < connections.Count; ++index)
            {
                if (connections[index].account != null && connections[index].account.username == account.username)
                {
                    return connections[index].character.name;
                }
            }
            return null;
        }

        public Account GetAccount(GameObject owner)
        {
            if (owner != null)
            {
                var pl = owner.GetComponent<PlayerCharacter>();
                if (pl != null)
                    return pl.account;
            }
            return null;
        }

        public Character GetCharacter(Account account)
        {
            for (int index = 0; index < connections.Count; ++index)
            {
                if (connections[index].account.username == account.username)
                {
                    return connections[index].character;
                }
            }
            return null;
        }

        public Deferred LogIn(string username, string password)
        {
            throw new NotImplementedException("Use the other version of LogIn");
        }

        public Deferred LogIn(IConnection connection, string username, string password)
        {
            var cond = Conductor.GetConductor();
            if (cond == null)
            {
                Debug.LogError("Can't log in without a conductor!");
                return Deferred.Fail(new Exception("Can't log in without a conductor!"));
            }
            var ssvc = cond.GetLocalServiceByInterface<IStorageService>();
            if (ssvc == null)
            {
                Debug.LogError("Can't log in without a storage service!");
                return Deferred.Fail(new Exception("Can't log in without a storage service!"));
            }
            return ssvc.Load<Account>(new List<string> { username }).AddCallback((res, p) =>
            {
                var acct = res as Account;
                if (acct == null)
                {
                    Debug.LogError("Error logging in " + username + ". Account not found.");
                    return Deferred.Fail(new Exception("Authentication error!"));
                }
                if (acct.password != password)
                {
                    Debug.LogError("Error logging in " + username + ". Password doesn't match!");
                    return Deferred.Fail(new Exception("Authentication error!"));
                }
                var c = connection as ConnectedUser;
                c.account = acct;
                if (c.player != null)
                {
                    var pc = c.player.GetComponent<PlayerCharacter>();
                    if (pc != null)
                    {
                        pc.account = acct;
                        pc.gameObject.name = "Player - " + acct.username;
                    }
                }
                return res;
            });
        }

        public Deferred GetCharacters(Account account)
        {
            var cond = Conductor.GetConductor();
            if (cond == null)
            {
                var res = new List<Character>();
                return Deferred.Succeed(res);
            }
            var ssvc = cond.GetLocalServiceByInterface<IStorageService>();
            if (ssvc == null)
            {
                var res = new List<Character>();
                return Deferred.Succeed(res);
            }
            var keys = StorageUtilities.GenerateGenericStorageKeys(StorageLevel.Account, ssvc, account, null);
            return ssvc.LoadList<Character>(keys);
        }

        public Deferred CreateCharacter(Account account, string name)
        {
            var newChar = new Character();
            newChar.name = name;
            return newChar.Save(account).AddCallback(delegate (object res, object[] args)
            {
                CATEventManager.FireGlobalEvent(gameObject, "CharactersChanged", null);
                return res;
            });
        }

        public Deferred DeleteCharacter(Account account, Character character)
        {
            var cond = Conductor.GetConductor();
            if (cond == null)
            {
                return Deferred.Succeed(false);
            }
            var ssvc = cond.GetLocalServiceByInterface<IStorageService>();
            if (ssvc == null)
            {
                return Deferred.Succeed(false);
            }
            return ssvc.Delete(character, account, character).AddCallback(delegate (object res, object[] args)
            {
                CATEventManager.FireGlobalEvent(gameObject, "CharactersChanged", null);
                return true;
            });
        }

        public Deferred SelectCharacter(Account account, Character character)
        {
            if (account == null) Debug.LogError("NULL ACCOUNT PASSED IN!");
            CATEventManager.FireGlobalEvent(gameObject, "BeforeSelectCharacter", character);
            for (int index = 0; index < connections.Count; ++index)
            {
                if (connections[index].account != null && connections[index].account.username == account.username)
                {
                    connections[index].character = character;
                    if (connections[index].player != null)
                    {
                        var pc = connections[index].player.GetComponent<PlayerCharacter>();
                        if (pc != null)
                        {
                            pc.character = character;
                            pc.gameObject.name = "Player - " + account.username + " - " + character.name;
                        }
                    }
                }
            }
            Debug.Log("Selecting " + account.username + " - " + character.name);
            CATEventManager.FireGlobalEvent(gameObject, "OnSelectCharacter", new AccountCharacter(account, character));
            return Deferred.Succeed(character);
        }

        public CharacterClass GetCharacterClass(string className)
        {
            for (int index = 0; index < classes.Count; ++index)
                if (classes[index].name == className)
                    return classes[index];
            return null;
        }

        public CharacterRace GetCharacterRace(string raceName)
        {
            for (int index = 0; index < races.Count; ++index)
                if (races[index].name == raceName)
                    return races[index];
            return null;
        }

        public CharacterDefinition GetCharacterDefinition()
        {
            return characterDefinition;
        }

        public Deferred CreateAccount(string username, string password, string email)
        {
            var account = new Account();
            account.username = username;
            account.password = password;
            account.email = email;
            return account.Save().AddCallback(delegate (object res, object[] args)
            {
                CATEventManager.FireGlobalEvent(gameObject, "AccountsChanged", null);
                return res;
            });
        }

        public List<CharacterClass> GetCharacterClasses()
        {
            return classes;
        }

        public List<CharacterRace> GetCharacterRaces()
        {
            return races;
        }

        public void SpawnPlayer(IConnection connection, GameObject obj)
        {
            var c = connection as ConnectedUser;
            Debug.Log("Connection " + connection + " c " + c);
            var pc = obj.GetComponent<PlayerCharacter>();
            if (pc != null)
            {
                obj.name = obj.name + " - " + pc.account.username + " - " + pc.character.name;
            }

            if (c.player == null)
            {
                NetworkServer.AddPlayerForConnection(c.connection, obj);
                c.player = obj;
            }
            else
            {
                Destroy(c.player);
                NetworkServer.ReplacePlayerForConnection(c.connection, obj);
                c.player = obj;
            }
            if (pc != null)
            {
                pc.account = c.account;
                pc.character = c.character;
            }
            obj.name = "Player - " + c.account.username + " - " + c.character.name;
        }

        public void OnClientConnected(NetworkConnection conn)
        {
            connections.Add(new ConnectedUser(conn));
        }

        public void OnClientDisconnected(NetworkConnection conn)
        {
            for (int index = 0; index < connections.Count; ++index)
            {
                var c = connections[index];
                if (c.connection == conn)
                {
                    connections.Remove(c);
                    break;
                }
            }
        }

        public ConnectedUser FindConnection(NetworkConnection conn)
        {
            for (int index = 0; index < connections.Count; ++index)
            {
                var c = connections[index];
                if (c.connection.connectionId == conn.connectionId)
                {
                    return c;
                }
            }
            return null;
        }

        public IConnection GetConnection(Account account)
        {
            for (int index = 0; index < connections.Count; ++index)
            {
                var c = connections[index];
                if (c.account != null && c.account.username == account.username)
                {
                    return c;
                }
            }
            Debug.LogWarning("Didn't find connection for account! " + account.username);
            return null;
        }

        public GameObject GetPlayer(Account account)
        {
            for (int index = 0; index < connections.Count; ++index)
            {
                var c = connections[index];
                if (c.account != null && c.account.username == account.username)
                {
                    return c.player;
                }
            }
            Debug.LogWarning("Didn't find player for account! " + account.username);
            return null;
        }
    }

    public class ConnectedUser : IConnection
    {
        public Account account;
        public Character character;
        public GameObject player;
        public NetworkConnection connection;

        public ConnectedUser(NetworkConnection conn)
        {
            connection = conn;
        }
    }
}