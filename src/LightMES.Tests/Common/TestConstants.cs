namespace LightMES.Tests.Common;

public static class TestConstants
{
    public static class Audit
    {
        public const string CreatedBy = "test.creator";
        public const string ModifiedBy = "test.modifier";
    }

    public static class Users
    {
        public const string DefaultUsername = "test.user";
        public const string DefaultPasswordHash = "ATWRIOJFLDKJ>>MFNDS..--t";
        public const string DefaultFullName = "Test User";
        public const string DefaultEmployeeNo = "EMP-000";
        public const string DefaultBadgeNo = "BADGE-000";
    }

    public static class Roles
    {
        public const string AdminName = "Aministrator";
        public const string AdminDescription = "系统超级管理员,拥有所有权限";
        public const string OperatorName = "Operator";
        public const string OperatorDescription = "产线操作工，仅拥有报工和查看权限";
    }

    public static class Equipments
    {
        public const string EquipmentCode = "CNC-01";
        public const string EquipmentName = "测试CNC #1";
        public const string Location = "车间A-001";
        public const string Description = "测试描述";
    }

    public static class Routes
    {
        public const string DefaultCode = "R-ASY-001";
        public const string DefaultName = "主装配工艺路线";
        public const string DefaultVersion = "V1.0";

        public static class Steps
        {
            public const string Step01Code = "OP10";
            public const string Step01Name = "上线扫描";
            public const string Step02Code = "OP20";
            public const string Step02Name = "视觉检查";
        }
    }

    public static class Materials
    {
        public const string DefaultCode = "m-cpu-i7";
        public const string ExpectedNormalizedCode = "M-CPU-I7";
        public const string DefaultName = "Intel Core I7 处理器";
        public const string DefaultSpec = "I7-13700K 16-Core";
        public const string DefaultUnit = "PCS";
    }

    public static class WorkOrders
    {
        public const string DefaultOrderNo = "WO-20231024-001";
        public const string ProductCode = "PROD-CON-01";
        public const string ProductName = "智能控制器主板";
        public const int DefaultPlanQty = 100;
    }

    public static class TrackWips
    {
        public const string DefaultSN = "SN-20260612-0001";
    }
}
