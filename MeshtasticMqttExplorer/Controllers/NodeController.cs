using Common.Context;
using Common.Extensions;
using Common.Extensions.Entities;
using Common.Models;
using Common.Services;
using Meshtastic.Protobufs;
using MeshtasticMqttExplorer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MeshtasticMqttExplorer.Controllers;

[ApiController]
[Route("api/node")]
public class NodeController(ILogger<ActionController> logger, IDbContextFactory<DataContext> contextFactory, IConfiguration configuration) : AController(logger)
{
    [HttpGet("search")]
    public async Task<NodeDto?> SearchNode(string value)
    {
        var context = await contextFactory.CreateDbContextAsync();
        var node = await context.Nodes
            .Include(p => p.PacketsFrom.Where(a => a.CreatedAt.Date == DateTime.UtcNow.Date && a.PacketDuplicated == null && a.ViaMqtt != true && a.HopStart > 0 && a.PortNum != PortNum.MapReportApp && a.To.NodeId == MeshtasticService.NodeBroadcast))
            .Search(value);

        if (node == null)
        {
            return null;
        }

        return new NodeDto
        {
            Id = node.Id,
            NodeId = node.NodeId,
            LastSeen = node.LastSeen?.ToFrench(),
            NodeIdString = node.NodeIdString,
            LongName = node.LongName,
            ShortName = node.ShortName,
            AllNames = node.AllNames,
            Role = node.Role,
            Link = $"{configuration.GetValue<string>("FrontUrl")}/node/{node.Id}",
            Stats = node.PacketsFrom.GroupBy(a => new
            {
                NodeId = a.From.Id,
                a.PortNum
            }, (grouped, p) => new PacketNodeType
            {
                NodeId = grouped.NodeId,
                PortNum = grouped.PortNum,
                Nb = p.Count(),
            }).OrderByDescending(p => p.Nb).ToList()
        };
    }
}