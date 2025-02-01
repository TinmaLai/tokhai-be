using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.SignalR;
using Org.BouncyCastle.Tls;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using z76_backend.Models;

public class ExcelFormHub : Hub
{
    // Dùng ConcurrentDictionary để lưu danh sách user an toàn
    private static ConcurrentBag<UserForm> Users = new();
    private static string tableData = "";
    private static PaginationInfo paginationInfo;
    private static string filterGroups = "";
    // Hàm kiểm tra và thêm key-value nếu không trùng
    static bool TryAddUnique(ConcurrentBag<UserForm> dict, UserForm value)
    {
        bool isExist = false;
        // Kiểm tra xem value có trùng với key hiện có hay không
        foreach (var item in dict)
        {
            if(item.userId == value.userId)
            {
                isExist = true;
                break;
            }
        }
        if (!isExist)
        {
            dict.Add(value);
        }

        return true;
    }
    // Hàm xóa key-value khi cả key và value trùng
    static ConcurrentBag<UserForm> TryRemoveMatching(ConcurrentBag<UserForm> dict, string connectId)
    {
        bool isExist = false;
        var userAfterRemove = new List<UserForm>();

        // Kiểm tra xem value có trùng với key hiện có hay không
        foreach (var user in dict)
        {
            if (user.connectionId == connectId)
            {
                isExist = true;
                break;
            }
        }
        if (isExist)
        {
            var newDict = dict.Where(x => x.connectionId != connectId).ToList();
            dict = new ConcurrentBag<UserForm>(newDict);
        }
            
        // Nếu không trùng, thêm cặp key-value mới vào
        return dict;
    }
    public async Task SendTableExportData(string user, string message)
    {
        tableData = message;
        await Clients.All.SendAsync("ReceiveTableExportData", user, message);
    }
    public async Task SendFilterGroups(string user, string message)
    {
        filterGroups = message;
        await Clients.All.SendAsync("ReceiveFilterGroups", user, message);
    }
    public async Task SendUserInForm(UserForm user)
    {
        // Nếu userId đã tồn tại, không thêm lại
        var newUser = new UserForm
        {
            userId = user.userId,
            userName = user.userName, // Đổi thành giá trị hợp lệ nếu có tên
            avatar = user.avatar,
            connectionId = Context.ConnectionId
        };
        //Users.Append(newUser);
        TryAddUnique(Users, newUser);
        await Clients.All.SendAsync("UpdateUserList", Users.ToList());

    }
    public async Task SendPaginationInfo(PaginationInfo clientPaginationInfo)
    {
        //Users.Append(newUser);
        paginationInfo = clientPaginationInfo;
        await Clients.All.SendAsync("ReceivePaginationInfo", paginationInfo);

    }
    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        await Clients.Client(connectionId).SendAsync("ReceiveTableExportData", "", tableData);
        await Clients.Client(connectionId).SendAsync("ReceivePaginationInfo", paginationInfo);
        await Clients.All.SendAsync("ReceiveFilterGroups", "", filterGroups);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        
        await base.OnDisconnectedAsync(exception);
        Console.WriteLine($"Client disconnected: {Context.ConnectionId}, Reason: {exception?.Message}");
        // Xóa user khi họ disconnect
        var userAfterDelete = TryRemoveMatching(Users, Context.ConnectionId);
        Users = userAfterDelete;
        // Cập nhật danh sách user đến tất cả client
        await Clients.All.SendAsync("UpdateUserList", Users.ToList());
    }
}

public class UserForm
{
    public string userId { get; set; }
    public string userName { get; set; }
    public string avatar { get; set; }
    public string connectionId { get; set; }
}
public class PaginationInfo
{
    public int itemsPerPage { get; set; }
    public int currentPage { get; set; }

}
