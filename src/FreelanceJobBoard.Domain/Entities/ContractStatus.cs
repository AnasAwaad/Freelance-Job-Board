using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreelanceJobBoard.Domain.Entities;
public class ContractStatus
{
    public int Id {  get; set; }
    public string Name { get; set; }
    public ICollection<Contract> Contracts { get; set; }
}