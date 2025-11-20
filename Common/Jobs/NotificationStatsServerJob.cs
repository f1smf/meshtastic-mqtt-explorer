using Common.Context;
using Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Common.Jobs;

public class NotificationStatsServerJob(ILogger<NotificationStatsServerJob> logger, IDbContextFactory<DataContext> contextFactory, IServiceProvider serviceProvider) : AJob(logger, contextFactory, serviceProvider)
{
    public async Task ExecuteAsync()
    {
        Logger.LogInformation("Notification stats début");
        
        var services = serviceProvider.CreateScope().ServiceProvider;
        var notificationService = services.GetRequiredService<NotificationService>();

        var mqttServerToSend = (await Context.Webhooks
            .Include(n => n.MqttServer)
            .Where(w => w.MqttServer != null && w.IncludeStats && w.Enabled && w.OnePerDay)
            .ToListAsync())
            .GroupBy(w => new { w.MqttServer, w.Url }, (key, webhooks) => webhooks.First());
        
        foreach (var webhook in mqttServerToSend)
        {
            await notificationService.MakeRequest(webhook, notificationService.GetTextStatsForServer(webhook.MqttServer!));
            Logger.LogInformation("Notification stats pour le serveur #{serverId} OK", webhook.MqttServerId);   
        }
    }
}