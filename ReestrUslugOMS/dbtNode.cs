//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ReestrUslugOMS
{
    using System;
    using System.Collections.Generic;
    
    public partial class dbtNode
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public dbtNode()
        {
            this.Formula = new HashSet<dbtFormula>();
            this.Next = new HashSet<dbtNode>();
            this.User = new HashSet<dbtUser>();
            this.Plan = new HashSet<dbtPlan>();
            this.Plan1 = new HashSet<dbtPlan>();
        }
    
        public int NodeId { get; set; }
        public string Order { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public bool ReadOnly { get; set; }
        public ReestrUslugOMS.enDataSource DataSource { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<dbtFormula> Formula { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<dbtNode> Next { get; set; }
        public virtual dbtNode Prev { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<dbtUser> User { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<dbtPlan> Plan { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<dbtPlan> Plan1 { get; set; }
    }
}
