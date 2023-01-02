using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GrpcService
{
    public class MatchData
    {
        int MAX_ROOM_COUNT_IN_SERVER = 10;

        Dictionary<UInt32, User> _aidxUserMap = new Dictionary<uint, User>();
        List<Room> _roomList = new List<Room>();
        List<ServerInfo> _serverList = new List<ServerInfo>();
        Dictionary<string, List<ServerInfo>> _regionServerMap = new Dictionary<string, List<ServerInfo>>();


        public RoomList GetRoomList(User user)
        {
            RoomList roomList = new RoomList();

            string userRegion = user.Region;
            List<ServerInfo> serverList;
            bool result = _regionServerMap.TryGetValue(userRegion, out serverList);
            if (result)
            {
                // 유저 리전에 방이 있음
                foreach (var serverInfo in serverList)
                {
                    roomList.MergeFrom(serverInfo.RoomList);
                }
            }
            else
            { 
                // 유저 리전에 방이 없음
            }

            return roomList;
        }


        public Room CreateRoom(CreatedRoomInfo createdRoomInfo)
        {
            string userRegion = createdRoomInfo.User.Region;
            List<ServerInfo> serverList;

            bool result = _regionServerMap.TryGetValue(userRegion, out serverList);
            if (result)
            {
                // 유저 리전에 방이 있음
                // 해당 리전의 서버 중, 가장 방이 적은 서버에 방 생성
                ServerInfo? minServer = null;
                foreach (var serverInfo in serverList)
                {
                    if (minServer == null)
                    {
                        minServer = serverInfo;
                        continue;
                    }
                    int minCount = minServer.RoomList.Room.Count;
                    int count = serverInfo.RoomList.Room.Count;

                    if (minCount > count) minServer = serverInfo;
                }

                //// 여기까지 오면 minServer는 무조건 존재
                // 아래의 경우 서버 증설 필요
                //if (minServer.RoomList.Room.Count >= MAX_ROOM_COUNT_IN_SERVER)
                //    return null;

                Room room = new Room();
                room.Key = "newRoomKey"; // 임의 생성해야함
                room.Name = createdRoomInfo.Name;
                room.MemberCount = 0;
                room.MinMemberCount = createdRoomInfo.MinMemberCount; // 검증
                room.MaxMemberCount = createdRoomInfo.MaxMemberCount; // 검증
                room.MapId = createdRoomInfo.MapId; // 검증

                minServer.RoomList.Room.Add(room);

                return room;
            }

            //해당 지역에 서버가 없음
            // 아래의 경우 서버 증설 필요
            return null;
        }


        public Empty EnterRoom(EnteredRoomInfo enteredRoomInfo)
        {
            User wannaEnterUser = enteredRoomInfo.User;

            string userRegion = wannaEnterUser.Region;
            List<ServerInfo> serverList;
            bool result = _regionServerMap.TryGetValue(userRegion, out serverList);
            if (result)
            {
                Room? wannaEnterRoom = null;
                // 유저 리전에 방이 있음
                foreach (var serverInfo in serverList)
                {
                    foreach (Room room in serverInfo.RoomList.Room)
                    {
                        if (room.Key == enteredRoomInfo.Key)
                        {
                            wannaEnterRoom = room;
                            break;
                        }
                    }
                }
            }
            else
            {
                // 유저 리전에 방이 없음
                return null;
            }

            return new Empty();
        }


        public ServerInfoList GetServerInfoList()
        {
            ServerInfoList serverInfoList = new ServerInfoList();


            return serverInfoList;
        }

        public Empty AddServer(ServerInfo serverInfo)
        {
            ServerInfo newServer = serverInfo;

            List<ServerInfo> serverList;
            bool result = _regionServerMap.TryGetValue(newServer.Region, out serverList);
            if (result)
            {
                serverList.Add(serverInfo);
            }
            else
            {
                // 유저 리전에 방이 없음
                serverInfo.RoomList = new RoomList();

                serverList = new List<ServerInfo>();
                serverList.Add(serverInfo);
                _regionServerMap.Add(serverInfo.Region, serverList);
            }

            return new Empty();
        }
    };

    class MatchImpl : MatchService.MatchServiceBase
    {

        MatchData _matchData = new MatchData();

        public override Task<RoomList> GetRoomList(User user, ServerCallContext context)
        {
            return Task.FromResult(_matchData.GetRoomList(user));
        }

        public override Task<Room> CreateRoom(CreatedRoomInfo createdRoomInfo, ServerCallContext context)
        {
            return Task.FromResult(_matchData.CreateRoom(createdRoomInfo));
        }


        public override Task<Empty> AddServer(ServerInfo serverInfo, ServerCallContext context)
        {
            return Task.FromResult(_matchData.AddServer(serverInfo));
        }

        //public override Task<ServerInfoList> GetServerInfoList(Empty request, ServerCallContext context)
        //{
        //    MatchData matchData = new MatchData();
        //    return Task.FromResult(matchData.GetServerInfoList());
        //}
    };

};
