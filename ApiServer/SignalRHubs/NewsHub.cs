﻿using ApiServer.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiServer.SignalRHubs
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class NewsHub : Hub
    {
        private NewsStore _newsStore;

        public NewsHub(NewsStore newsStore)
        {
            _newsStore = newsStore;
        }

        public Task Send(NewsItem newsItem)
        {
            if(!_newsStore.GroupExists(newsItem.NewsGroup))
            {
                throw new System.Exception("cannot send a news item to a group which does not exist.");
            }

            _newsStore.CreateNewItem(newsItem);
            return Clients.Group(newsItem.NewsGroup).InvokeAsync("Send", newsItem);
        }

        public async Task JoinGroup(string groupName)
        {
            if (!_newsStore.GroupExists(groupName))
            {
                throw new System.Exception("cannot join a group which does not exist.");
            }

            await Groups.AddAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).InvokeAsync("JoinGroup", groupName);

            var history = _newsStore.GetAllNewsItems(groupName);
            await Clients.Client(Context.ConnectionId).InvokeAsync("History", history);
        }

        public async Task LeaveGroup(string groupName)
        {
            if (!_newsStore.GroupExists(groupName))
            {
                throw new System.Exception("cannot leave a group which does not exist.");
            }

            await Clients.Group(groupName).InvokeAsync("LeaveGroup", groupName);
            await Groups.RemoveAsync(Context.ConnectionId, groupName);
        }
    }
}
