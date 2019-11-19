using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TwitchClient.Core;

namespace TwitchClient
{
    class ApiRequest
    {
        public async Task<String> ParseM3UAsync(Uri url, string quality)
        {
            HttpClient http = new HttpClient();

            var data = await http.GetStringAsync(url);

            var lines = data.Split('\n');

            if (lines.Any())
            {
                if (lines[0] != "#EXTM3U")
                {
                    return "null";
                }

                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains("#EXT-X-STREAM") && lines[i].Contains("VIDEO=\"" + quality + "\""))
                    {
                        return lines[i + 1];
                    }
                }
            }

            return "null";
        }
        public async Task<Uri> UriAsync(string login)
        {
            using (var httpClient = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("GET"), $"https://api.twitch.tv/api/channels/{login}/access_token");
                request.Headers.Add("Client-ID", "85lcqzxpb9bqu9z6ga1ol55du");
                request.Headers.Add("Accept", "application/vnd.twitchtv.v3+json");
                var response = await httpClient.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();
                var json = JsonConvert.DeserializeObject<VideoURIModel>(result);
                string token = json.token;
                string sig = json.sig;
                return new Uri($"https://usher.ttvnw.net/api/channel/hls/{login}.m3u8?allow_source=true&baking_bread=false&baking_brownies=false&baking_brownies_timeout=1050&fast_bread=true&p=844740&player_backend=mediaplayer&playlist_include_framerate=true&reassignments_supported=false&rtqos=control&sig={sig}&token={token}&cdm=wv");
            }
        }
        public async Task<UserModel> GetUserInfoAsync(string id)
        {
            HttpClient httpClient = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("GET"), $"https://api.twitch.tv/helix/users?id={id}");
            request.Headers.Add("Client-ID", "0pje11teayzq9z2najlxgdcc5d2dy1");
            var response = await httpClient.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<UserModel>(result);
            return json;
        }
        public async Task<StreamModel> GetStreamInfoAsync()
        {
            HttpClient httpClient = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("GET"), "https://api.twitch.tv/helix/streams");
            request.Headers.Add("Client-ID", "0pje11teayzq9z2najlxgdcc5d2dy1");
            var response = await httpClient.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<StreamModel>(result);
            return json;
        }
        public async Task<StreamModel> GetStreamInfoAsync(string param)
        {
            HttpClient httpClient = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("GET"), $"https://api.twitch.tv/helix/streams?{param}");
            request.Headers.Add("Client-ID", "0pje11teayzq9z2najlxgdcc5d2dy1");
            var response = await httpClient.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<StreamModel>(result);
            return json;
        }
        public async Task<GameModel> GetGameInfoAsync(string gameID)
        {
            HttpClient httpClient = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("GET"), $"https://api.twitch.tv/helix/games?id={gameID}");
            request.Headers.Add("Client-ID", "0pje11teayzq9z2najlxgdcc5d2dy1");
            var response = await httpClient.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<GameModel>(result);
            return json;
        }
        public async Task<TopGamesModel> GetTopGameAsync()
        {
            HttpClient httpClient = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("GET"), $"https://api.twitch.tv/helix/games/top");
            request.Headers.Add("Client-ID", "0pje11teayzq9z2najlxgdcc5d2dy1");
            var response = await httpClient.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<TopGamesModel>(result);
            return json;
        }
        public async Task<SearchStreamModel> SearchStream(string param)
        {
            HttpClient httpClient = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("GET"), $"https://api.twitch.tv/kraken/search/streams?query={param}");
            request.Headers.Add("Client-ID", "0pje11teayzq9z2najlxgdcc5d2dy1");
            request.Headers.Add("Accept", "application/vnd.twitchtv.v5+json");
            var response = await httpClient.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<SearchStreamModel>(result);
            return json;
        }
        public async Task<SearchStreamModel> GetKrakenStreams()
        {
            HttpClient httpClient = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("GET"), $"https://api.twitch.tv/kraken/streams/?limit=10");
            request.Headers.Add("Client-ID", "0pje11teayzq9z2najlxgdcc5d2dy1");
            request.Headers.Add("Accept", "application/vnd.twitchtv.v5+json");
            var response = await httpClient.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<SearchStreamModel>(result);
            return json;
        }
        public async Task<SearchStreamModel> GetKrakenStreams(string ChannelID)
        {
            HttpClient httpClient = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("GET"), $"https://api.twitch.tv/kraken/streams/{ChannelID}");
            request.Headers.Add("Client-ID", "0pje11teayzq9z2najlxgdcc5d2dy1");
            request.Headers.Add("Accept", "application/vnd.twitchtv.v5+json");
            var response = await httpClient.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<SearchStreamModel>(result);
            return json;
        }

        public async Task<ProfileModel> GetProfileAsync(string OAuth)
        {
            HttpClient httpClient = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("GET"), $"https://api.twitch.tv/kraken/user");
            request.Headers.Add("Client-ID", "0pje11teayzq9z2najlxgdcc5d2dy1");
            request.Headers.Add("Accept", "application/vnd.twitchtv.v5+json");
            request.Headers.Add("Authorization", "OAuth " + OAuth);
            var response = await httpClient.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<ProfileModel>(result);
            return json;
        }
        public async Task FollowChannelAsync(string userID, string channelID, string OAuth)
        {
            HttpClient httpClient = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PUT"), $"https://api.twitch.tv/kraken/users/{userID}/follows/channels/{channelID}");
            request.Headers.Add("Client-ID", "0pje11teayzq9z2najlxgdcc5d2dy1");
            request.Headers.Add("Accept", "application/vnd.twitchtv.v5+json");
            request.Headers.Add("Authorization", "OAuth " + OAuth);
            var response = await httpClient.SendAsync(request);
            Debug.WriteLine("You are succesfully following a channel");
        }

    }
}
