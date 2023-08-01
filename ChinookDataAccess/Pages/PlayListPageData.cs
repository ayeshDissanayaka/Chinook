using ChinookDataAccess.Models;
using ChinookDataAccess.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChinookDataAccess.Pages.Interfaces;
using ChinookDataAccess.ClientModels;
using Microsoft.Extensions.Logging;

namespace ChinookDataAccess.Pages
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
        public async Task<List<UserPlaylist>> getPlayListByUser(string userId)
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var userPlayList = await dbContext.UserPlaylists.Where(a => a.UserId == userId.Trim()).ToListAsync();
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
                Tracks = p.Tracks.Select(t => new ChinookDataAccess.ClientModels.PlaylistTrack()
                {
                    AlbumTitle = t.Album.Title,
                    ArtistName = t.Album.Artist.Name,
                    TrackId = t.TrackId,
                    TrackName = t.Name,
                    IsFavorite = t.Playlists.Where(p => p.UserPlaylists.Any(up => up.UserId == userId && up.Playlist.Name == "Favorites")).Any()
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
