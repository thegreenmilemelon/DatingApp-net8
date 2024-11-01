using System;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;


namespace API.Data;

public class LikesRepository(DataContext context, IMapper mapper) : ILikesRepository
{
    public void AddLike(UserLike like)
    {
        context.Likes.Add(like);
    }

    public void DeleteLike(UserLike like)
    {
        context.Likes.Remove(like);
    }

    //gives the list of just target userIDs the current user has liked
    public async Task<IEnumerable<int>> GetCurrentUserLikeIds(int currentUserId)
    {
        return await context.Likes.Where(x => x.SourceUserId == currentUserId).Select(x => x.TargetUserId).ToListAsync();
    }

    //checks if the specific source userIDs has liked the specific target user
    public async Task<UserLike?> GetUserLike(int sourceUserId, int targetUserId)
    {
        return await context.Likes.FindAsync(sourceUserId, targetUserId);
    }

    // public async Task<IEnumerable<MemberDto>> GetUserLikes(string predicate, int userId)
    // {
    //     var likes = context.Likes.AsQueryable();

    //     switch (predicate)
    //     {
    //         case "liked":
    //         return await likes.Where(x => x.SourceUserId == userId)
    //         .Select(x => x.TargetUser)
    //         .ProjectTo<MemberDto>(mapper.ConfigurationProvider).ToListAsync();

    //         case "likedBy":
    //         return await likes.Where(x => x.TargetUserId == userId)
    //         .Select(x => x.SourceUser)
    //         .ProjectTo<MemberDto>(mapper.ConfigurationProvider).ToListAsync();

    //         default:
    //          var likeIds = await GetCurrentUserLikeIds(userId);
    //          return await likes.Where(x => x.TargetUserId == userId && likeIds.Contains(x.SourceUserId))
    //          .Select(x => x.SourceUser)
    //          .ProjectTo<MemberDto>(mapper.ConfigurationProvider).ToListAsync();
    //     }

    // }

    public async Task<PagedList<MemberDto>> GetUserLikes(LikesParams likesParams)
    {
        var likes = context.Likes.AsQueryable();

        IQueryable<MemberDto> query;

        switch (likesParams.Predicate)
        {
            case "liked":
                query = likes.Where(x => x.SourceUserId == likesParams.UserId)
                .Select(x => x.TargetUser)
                .ProjectTo<MemberDto>(mapper.ConfigurationProvider);
                break;

            case "likedBy":
                query = likes.Where(x => x.TargetUserId == likesParams.UserId)
                .Select(x => x.SourceUser)
                .ProjectTo<MemberDto>(mapper.ConfigurationProvider);
                break;

            default:
                var likeIds = await GetCurrentUserLikeIds(likesParams.UserId);
                query = likes.Where(x => x.TargetUserId == likesParams.UserId && likeIds.Contains(x.SourceUserId))
                .Select(x => x.SourceUser)
                .ProjectTo<MemberDto>(mapper.ConfigurationProvider);
                break;
        }

        return await PagedList<MemberDto>.CreateAsync(query, likesParams.PageNumber, likesParams.PageSize);

    }

    public async Task<bool> SaveChanges()
    {
        return await context.SaveChangesAsync() > 0;

    }
}
