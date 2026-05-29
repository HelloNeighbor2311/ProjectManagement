using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.Statuses;

public class UpdateStatusDto
{
    public Guid Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;
}