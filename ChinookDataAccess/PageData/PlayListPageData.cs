using ChinookDataAccess.ClientModels;
using ChinookDataAccess.Contexts;
using ChinookDataAccess.Models;
using ChinookDataAccess.PageData.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChinookDataAccess.PageData
{
    public class PlayListPageData : IPlayListPageData
    {
        private readonly IDbContextFactory<ChinookContext> _dbContextFactory;
        private readonly ILogger<string> _logger;
        public PlayListPageData(IDbContextFactory<ChinookContext> dbContextFactory,ILogger<string> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }
        public async Task<List<UserPlaylistClient>?> getPlayListByUser(string userId)
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var userPlayList = new List<UserPlaylist>();
            if (!string.IsNullOrEmpty(userId))
            {
                userPlayList = dbContext.UserPlaylists.Where(a => a.UserId == userId.Trim()).ToList();
            }
            return mapUserPlayListClient(userPlayList);
        }

        public async Task<List<UserPlaylistClient>?> getFilteredPlayListByUser(long trackId, string userId)
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var TrackPlayList = new List<Models.PlayListTrack>();
            var userPlayList = new List<UserPlaylistClient>();
            var userSelectedPlayLists = new List<UserPlaylistClient>();
            var trackOwnedPlayLists= dbContext.PlayListTracks.Where(a=>a.TrackId.Equals(trackId)).ToList();
            var userPlayListData = dbContext.UserPlaylists.Where(a => a.UserId == userId.Trim()).ToList();
            userPlayList = mapUserPlayListClient(userPlayListData);
            if (trackOwnedPlayLists.Any())
            {
                var containedUserPlayList = userPlayList.Where(a => !trackOwnedPlayLists.Select(c=>c.PlayListId).Contains(a.PlaylistId)).ToList();
                userPlayList = containedUserPlayList;
                return userPlayList;
            }

            return userPlayList;
        }

        public async Task<Models.Playlist> getPlayListById(long playListId)
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            return await dbContext.Playlists.SingleOrDefaultAsync(a => a.PlaylistId == playListId);
        }
        public async Task<ClientModels.Playlist> getPlayListDetailsByUserId(string userId,long PlaylistId)
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var playListData= dbContext.Playlists
            .Include(a => a.Tracks).ThenInclude(a => a.Album).ThenInclude(a => a.Artist)
            .Where(p => p.PlaylistId == PlaylistId)
            .Select(p => new ChinookDataAccess.ClientModels.Playlist()
            {
                Name = p.Name,
                Tracks = p.Tracks.Select(t => new ChinookDataAccess.ClientModels.PlaylistTrackClient()
                {
                    AlbumTitle = t.Album.Title,
                    ArtistName = t.Album.Artist.Name,
                    TrackId = t.TrackId,
                    TrackName = t.Name,
                    IsFavorite = t.Playlists.Where(p => p.UserPlaylists.Any(up => up.UserId == userId && up.PlaylistId == 19)).Any()
                }).ToList()
            })
            .FirstOrDefault();

            return playListData;
        }

        public async Task<string> postTracksForPlayList(string playListName,long trackId,string? userId)
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var playListRecs = await getExsistingPlayLists(playListName)??null;
            if (playListRecs != null)
            {
                try
                {
                    var enteredOnUsersPlayList = await getPlayListByUser(userId);
                    if (enteredOnUsersPlayList != null)
                    {
                        if (!enteredOnUsersPlayList.Where(a => a.PlaylistId == playListRecs.PlaylistId).Any())
                        {
                            await addPlayListToUser(userId, playListRecs.PlaylistId);
                        }
                    }
                    var userPlayListTracks = mapPlayListTracks(playListRecs.PlaylistId, trackId);
                    await dbContext.PlayListTracks.AddAsync(userPlayListTracks);
                    await dbContext.SaveChangesAsync();
                    return "Sucess";
                }
                catch (Exception ex)
                {
                    _logger.LogError("PlayListTrackError",ex.Message);
                    return "Error on Saving...";
                }
            }
            else
            {
                var response=await addPlayList(playListName);
                if (response != null)
                {
                    await addPlayListToUser(userId, response.PlaylistId);
                    await this.postTracksForPlayList(playListName, trackId,userId);
                }
                else
                {
                    throw new InvalidDataException();
                }
               
            }
            return "Sucess";
        }


        public async Task<string?> removeTrackFromPlayList(string playListName, long trackId, string? userId)
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            try
            {
                var playListObj=dbContext.Playlists.Where(a=>a.Name==playListName).FirstOrDefault();
                if (playListObj != null)
                {
                    var playListTrackObj = dbContext.PlayListTracks.Where(a => a.TrackId == trackId && a.PlayListId == playListObj.PlaylistId).FirstOrDefault();
                    if (playListTrackObj != null)
                    {
                        dbContext.PlayListTracks.Remove(playListTrackObj);
                        await dbContext.SaveChangesAsync();
                    }
                }

                return "Sucess";
            }
            catch (Exception ex)
            {
                _logger.LogError("PlayListError", ex.Message);
                return null;
            }

        }

        #region Mappers
        private PlayListTrack mapPlayListTracks (long playListId,long trackId)
        {
            var playListTrackObj = new PlayListTrack()
            {
                PlayListId = playListId,
                TrackId = trackId
            };
            return playListTrackObj;
        }
        private Models.Playlist mapPlayList(string playListName, long playListId=0)
        {
            var playListObj = new Models.Playlist()
            {
                PlaylistId=playListId,
                Name= playListName,
            };
            return playListObj;
        }
        private Models.UserPlaylist mapPlayListToUser(string userId, long playListId = 0)
        {
            var playListUserObj = new Models.UserPlaylist()
            {
                PlaylistId = playListId,
                UserId = userId,
            };
            return playListUserObj;
        }

        private List<UserPlaylistClient> mapUserPlayListClient(List<UserPlaylist> modelUserPlayList)
        {
            var userPlayList=new List<UserPlaylistClient>();
            foreach(var playlist in modelUserPlayList)
            {
                var newUserPlayList = new UserPlaylistClient()
                {
                    PlaylistId = playlist.PlaylistId,
                    UserId = playlist.UserId
                };
                userPlayList.Add(newUserPlayList);
            }
            return userPlayList;
        }
       
        #endregion
        private async Task<Models.Playlist> getExsistingPlayLists(string playListName)
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var listOfExsistingPlayLists=dbContext.Playlists.Where(a=>a.Name==playListName.Trim()).FirstOrDefault();
            return listOfExsistingPlayLists;
        }

        private async Task<Models.Playlist?> addPlayList(string playListName)
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            try
            {
                long? intIdt = dbContext.Playlists.Max(u => (long?)u.PlaylistId)+1;
                var playListObj = mapPlayList(playListName, (long)intIdt);
                await dbContext.Playlists.AddAsync(playListObj);
                await dbContext.SaveChangesAsync();
                return playListObj;
            }
            catch (Exception ex)
            {
                _logger.LogError("PlayListError",ex.Message);
                return null;
            }

        }
        private async Task<string> addPlayListToUser(string userId,long playListId)
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            try
            {
                var playListUserObj = mapPlayListToUser(userId, playListId);
                await dbContext.UserPlaylists.AddAsync(playListUserObj);
                await dbContext.SaveChangesAsync();
                return "Sucess";
            }
            catch (Exception ex)
            {
                throw new InvalidDataException();
            }

        }
    }
}
