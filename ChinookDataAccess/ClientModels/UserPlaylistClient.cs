using ChinookDataAccess.Models;

namespace ChinookDataAccess.ClientModels;

public class UserPlaylistClient
{
    public string UserId { get; set; }
    public long PlaylistId { get; set; }
    public ChinookUser User { get; set; }
    public Playlist Playlist { get; set; }
}
