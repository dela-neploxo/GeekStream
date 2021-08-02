﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeekStream.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace GeekStream.Infrastructure.Data
{
    public class ChatRepository
    {
        private readonly AppDbContext _context;

        public ChatRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Message> CreateMessage(int chatId, string message, string userId)
        {
            var createMessage = new Message
            {
                ChatId = chatId,
                Text = message,
                Name = userId,
                Timestamp = DateTime.Now
            };

            _context.Messages.Add(createMessage);
            await _context.SaveChangesAsync();

            return createMessage;
        }

        public async Task<int> CreatePrivateRoom(string rootId, string targetId)
        {
            var chat = new Chat
            {
                Type = ChatType.Private
            };

            List<ChatUser> users = new List<ChatUser>();
            users.Add(new ChatUser
            {
                UserId = targetId
            });

            users.Add(new ChatUser
            {
                UserId = rootId
            });

            chat.Users = users;

            _context.Chats.Add(chat);

            await _context.SaveChangesAsync();

            return chat.Id;
        }

        public async Task CreateRoom(string name, string userId)
        {
            var chat = new Chat
            {
                Name = name,
                Type = ChatType.Room
            };

            var users = new List<ChatUser>();

            users.Add(new ChatUser
            {
                UserId = userId,
            });
            chat.Users = users;

            _context.Chats.Add(chat);

            await _context.SaveChangesAsync();
        }

        public Chat GetChat(int id)
        {
            return _context.Chats
                .Include(x => x.Messages)
                .FirstOrDefault(x => x.Id == id);
        }

        public IEnumerable<Chat> GetChats(string userId)
        {
            return _context.Chats
                .Include(x => x.Users)
                .Where(x => x.Users
                    .All(y => y.UserId != userId))
                .ToList();
        }

        public IEnumerable<Chat> GetPrivateChats(string userId)
        {
            return _context.Chats
                   .Include(x => x.Users)
                       .ThenInclude(x => x.User)
                   .Where(x => x.Type == ChatType.Private
                       && x.Users
                           .Any(y => y.UserId == userId))
                   .ToList();
        }

        public async Task JoinRoom(int chatId, string userId)
        {
            var chatUser = new ChatUser
            {
                ChatId = chatId,
                UserId = userId,
            };

            _context.ChatUsers.Add(chatUser);

            await _context.SaveChangesAsync();
        }
    }
}