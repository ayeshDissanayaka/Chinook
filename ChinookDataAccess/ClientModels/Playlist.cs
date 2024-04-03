namespace ChinookDataAccess.ClientModels;

public class Playlist
{
    public string Name { get; set; }
    public List<PlaylistTrackClient> Tracks { get; set; }
}