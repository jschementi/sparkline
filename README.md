Sample Usage
============ 

XAML:

    <schementi:Sparkline x:Name="MySparkline" />

Add values:

    this.MySparkline.AddTimeValue(double);

The implementation is very basic; Sparkline.AddTimeValue constructs a point at
the next time interval and adds it to a Polyline. You can control everything 
about the line's visuals, including adding visible points along the line and 
showing horizontal lines for the latest/highest/lowest values. 

The source builds assemblies for both .NET 4.0 and Silverlight 4.