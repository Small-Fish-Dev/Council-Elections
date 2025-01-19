using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

public struct ChartDataSets
{
	[JsonPropertyName( "backgroundColor" )]
	public string[] BackgroundColor { get; set; }

	[JsonPropertyName( "data" )]
	public int[] Data { get; set; }

	public ChartDataSets( string[] backgroundColor, int[] data )
	{
		BackgroundColor = backgroundColor;
		Data = data;
	}
}

public struct ChartData
{
	[JsonPropertyName( "labels" )]
	public string[] Labels { get; set; }

	[JsonPropertyName( "datasets" )]
	public ChartDataSets[] DataSets { get; set; }

	public ChartData( string[] labels, ChartDataSets[] dataSets )
	{
		Labels = labels;
		DataSets = dataSets;
	}
}

public struct ChartFont
{
	[JsonPropertyName( "resizable" )]
	public bool Resizable { get; set; }

	[JsonPropertyName( "minSize" )]
	public int MinSize { get; set; }

	[JsonPropertyName( "maxSize" )]
	public int MaxSize { get; set; }

	public ChartFont( bool resizable, int minSize, int maxSize )
	{
		Resizable = resizable;
		MinSize = minSize;
		MaxSize = maxSize;
	}
}

public struct ChartOutlabels
{
	[JsonPropertyName( "text" )]
	public string Text { get; set; }

	[JsonPropertyName( "color" )]
	public string Color { get; set; }

	[JsonPropertyName( "stretch" )]
	public int Stretch { get; set; }

	[JsonPropertyName( "font" )]
	public ChartFont Font { get; set; }

	public ChartOutlabels( string text, string color, int stretch, ChartFont font )
	{
		Text = text;
		Color = color;
		Stretch = stretch;
		Font = font;
	}
}

public struct ChartPlugins
{
	[JsonPropertyName( "legend" )]
	public bool Legend { get; set; }

	[JsonPropertyName( "outlabels" )]
	public ChartOutlabels Outlabels { get; set; }

	public ChartPlugins( bool legend, ChartOutlabels outlabels )
	{
		Legend = legend;
		Outlabels = outlabels;
	}
}

public struct ChartOptions
{
	[JsonPropertyName( "plugins" )]
	public ChartPlugins Plugins { get; set; }

	public ChartOptions( ChartPlugins plugins )
	{
		Plugins = plugins;
	}
}

public struct Chart
{
	[JsonPropertyName( "type" )]
	public string ChartType { get; set; }

	[JsonPropertyName( "data" )]
	public ChartData Data { get; set; }

	[JsonPropertyName( "options" )]
	public ChartOptions Options { get; set; }

	public Chart( ChartEntry[] entries )
	{
		ChartType = "outlabeledPie";

		var labels = entries.Select( e => e.Label ).ToArray();
		var backgroundColors = entries.Select( e => e.Background.Hex ).ToArray();
		var data = entries.Select( e => e.Amount ).ToArray();

		Data = new ChartData(
			labels,
			new[] { new ChartDataSets( backgroundColors, data ) }
		);

		Options = new ChartOptions(
			new ChartPlugins(
				legend: false,
				outlabels: new ChartOutlabels(
					text: "%l %p",
					color: "white",
					stretch: 35,
					font: new ChartFont(
						resizable: true,
						minSize: 12,
						maxSize: 18
					)
				)
			)
		);
	}

	public static string CreateExampleJson()
	{
		var entries = new[]
		{
			new ChartEntry("ubre", new Color( 214, 0, 25), 1),
			new ChartEntry("ducc", new Color(114, 0, 214), 1),
			new ChartEntry("Kaydax",new Color( 0, 178, 214), 1),
			new ChartEntry("Grodbert", new Color( 214, 157, 0), 1),
			new ChartEntry("Golden G. Godfrey",new Color( 71, 214, 0),  9)
		};

		var chart = new Chart( entries );
		Log.Info( chart );
		return JsonSerializer.Serialize( chart );
	}
}

public struct ChartEntry
{
	public string Label { get; set; }
	public Color Background { get; set; }
	public int Amount { get; set; }

	public ChartEntry( string label, Color background, int amount )
	{
		Label = label;
		Background = background;
		Amount = amount;
	}
}

public class QuickChartApi
{
	public static Texture GetChartImage( string chartJson )
	{
		try
		{
			var baseUrl = "https://quickchart.io/chart";
			var url = $"{baseUrl}?c={Uri.EscapeDataString( chartJson )}";
			var texture = Texture.Load( url );

			return texture;
		}
		catch ( Exception ex )
		{
			Log.Info( $"Error fetching chart image: {ex.Message}" );
			return null;
		}
	}
}
