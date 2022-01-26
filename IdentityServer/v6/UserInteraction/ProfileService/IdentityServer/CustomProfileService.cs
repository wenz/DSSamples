﻿using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Test;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServerHost
{
    // The IProfileService lets IdentityServer know what claims
    // to include in tokens for a user.
    //
    // if you're using ASP.NET Identity for your user database, then we provide
    // a ProfileService<TUser> base class that you might want to derive from rather
    // than implementing IProfileService from scratch.
    public class CustomProfileService : IProfileService
    {
        // in this sample, the TestUserStore is our user "database"
        private readonly TestUserStore _users;

        public CustomProfileService(TestUserStore users)
        {
            _users = users;
        }

        // GetProfileDataAsync is what controls what claims are issued in the response
        // the sample code below shows a few different approaches, and you can adjust 
        // these based on your needs and requirements.
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            // context holds information about the request, the user, the client, the scopes, and the claims being requested
            // context.Subject is the user for whom the result is request is being made
            // context.Subject.Claims is the claims collection from the user's session cookie at login time
            // context.IssuedClaims is the collection of claims that your logic has decided to return in the response

            // context.RequestedClaimTypes represents the claims requested based on the resources requested and the
            // corresponding UserClaims configured on those resources (IdentityResource, ApiScope, and/or ApiResource)
            if (context.RequestedClaimTypes.Any())
            {
                // AddRequestedClaims will inspect the claims passed and only add the ones 
                // that match the claim types in the RequestedClaimTypes collection.
                context.AddRequestedClaims(context.Subject.Claims);

                // this adds any claims that were requested from the claims in the user store
                var user = _users.FindBySubjectId(context.Subject.GetSubjectId());
                if (user != null)
                {
                    context.AddRequestedClaims(user.Claims);
                }
            }

            // this checks if the user's session cookie contains a "picture" claim
            // and if present we add it to the result (if it's not already in there from above, possibly due to RequestedClaimTypes)
            // notice this is always done, regardless of the RequestedClaimTypes, which means
            // the result will always contains this claim even if not requested.
            if (!context.IssuedClaims.Any(x => x.Type == "picture"))
            {
                var picture = context.Subject.FindFirst("picture");
                if (picture != null)
                {
                    context.IssuedClaims.Add(picture);
                }
            }

            return Task.CompletedTask;
        }

        // IsActiveAsync is called to ask your custom logic if the user is still "active".
        // If the user is not "active" then no new tokens will be created for them, even 
        // if the user has an active session with IdentityServer.
        public Task IsActiveAsync(IsActiveContext context)
        {
            // as above, context.Subject is the user for whom the result is request is being made
            // setting context.IsActive to false allows your logic to indicate that the token should not be created
            // context.IsActive defauts to true

            return Task.CompletedTask;
        }
    }
}
