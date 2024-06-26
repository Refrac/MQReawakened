﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Server.Base.Accounts.Helpers;
using Server.Base.Database.Accounts;
using Server.Reawakened.Core.Services;
using Server.Reawakened.Database.Users;
using Web.Launcher.Extensions;
using Web.Launcher.Models;

namespace Web.Launcher.Controllers.API.JSON.DLC;

[Route("api/json/dlc/login")]
public class LoginController(AccountHandler accHandler, UserInfoHandler userInfoHandler, GetServerAddress getSA,
    LauncherRConfig rConfig, PasswordHasher passwordHasher, LauncherRwConfig config, ILogger<LoginController> logger) : Controller
{
    [HttpPost]
    public IActionResult HandleLogin([FromForm] string username, [FromForm] string password)
    {
        username = username?.Trim();
        password = password?.Trim();

        var hashedPw = passwordHasher.GetPassword(username, password);

        var account = accHandler.GetAccountFromUsername(username);

        if (account == null)
        {
            logger.LogError("Could not find account for {Username}", username);
            return BadRequest();
        }

        var userInfo = userInfoHandler.GetUserFromId(account.Id);

        if (userInfo == null)
        {
            logger.LogError("Could not find user info for {Username} (ID: {Id})", username, account.Id);
            return BadRequest();
        }

        if (account.Password != hashedPw && userInfo.AuthToken != password)
        {
            logger.LogError("Account password does not match for {Username} (ID: {Id})", username, account.Id);
            return Unauthorized();
        }

        var loginData = account.GetLoginData(userInfo, getSA, config, rConfig);

        return Ok(JsonConvert.SerializeObject(loginData));
    }
}
