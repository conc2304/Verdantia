// This class represents a data point for a metric, storing its value and the associated time in a YearMonth format (e.g., 202401.5 for mid-January 2024).
public class MetricData
{
    public float Value { get; set; }
    public float YearMonth { get; set; } // Format: YYYYMM.d
}
