using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmailService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RabbitMqConfigs");

            migrationBuilder.CreateTable(
                name: "RetryOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    MaxAttempts = table.Column<int>(type: "integer", nullable: false),
                    InitialDelaySeconds = table.Column<int>(type: "integer", nullable: false),
                    MaxDelaySeconds = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetryOptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RabbitMqOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Host = table.Column<string>(type: "text", nullable: false),
                    Port = table.Column<int>(type: "integer", nullable: false),
                    VirtualHost = table.Column<string>(type: "text", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    Exchange = table.Column<string>(type: "text", nullable: false),
                    ExchangeType = table.Column<string>(type: "text", nullable: false),
                    Queue = table.Column<string>(type: "text", nullable: false),
                    RoutingKey = table.Column<string>(type: "text", nullable: false),
                    DeadLetterExchange = table.Column<string>(type: "text", nullable: false),
                    DeadLetterQueue = table.Column<string>(type: "text", nullable: false),
                    DeadLetterRoutingKey = table.Column<string>(type: "text", nullable: false),
                    PrefetchCount = table.Column<int>(type: "integer", nullable: false),
                    PublisherConfirms = table.Column<bool>(type: "boolean", nullable: false),
                    PersistentMessages = table.Column<bool>(type: "boolean", nullable: false),
                    RetryId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RabbitMqOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RabbitMqOptions_RetryOptions_RetryId",
                        column: x => x.RetryId,
                        principalTable: "RetryOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RabbitMqOptions_RetryId",
                table: "RabbitMqOptions",
                column: "RetryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RabbitMqOptions");

            migrationBuilder.DropTable(
                name: "RetryOptions");

            migrationBuilder.CreateTable(
                name: "RabbitMqConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Exchange = table.Column<string>(type: "text", nullable: false),
                    Host = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    Port = table.Column<int>(type: "integer", nullable: false),
                    QueueName = table.Column<string>(type: "text", nullable: false),
                    RoutingKey = table.Column<string>(type: "text", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RabbitMqConfigs", x => x.Id);
                });
        }
    }
}
