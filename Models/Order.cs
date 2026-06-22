using System;
using System.Collections.Generic;

namespace NorthwindApp.Models;

public partial class Order
{
    public short OrderId { get; set; }

    public string? CustomerId { get; set; }

    public short? EmployeeId { get; set; }

    public DateOnly? OrderDate { get; set; }

    public DateOnly? RequiredDate { get; set; }

    public DateOnly? ShippedDate { get; set; }

    public short? ShipVia { get; set; }

    public float? Freight { get; set; }

    public string? ShipName { get; set; }

    public string? ShipAddress { get; set; }

    public string? ShipCity { get; set; }

    public string? ShipRegion { get; set; }

    public string? ShipPostalCode { get; set; }

    public string? ShipCountry { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Shipper? ShipViaNavigation { get; set; }
}
