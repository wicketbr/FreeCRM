var dotNetHelper;

export function LoadHighchartsResources() {
    // This function checks to see if the Highcharts libraries have been loaded.
    // If so, this immediately invokes the OnHighchartsLoaded method in the Blazor
    // component. If not, the libraries are loaded and then the method is invoked.
    if (typeof (Highcharts) == "object") {
        dotNetHelper.invokeMethodAsync("OnHighchartsLoaded");
    } else {
        // AMD Workaround: Monaco's vs/loader.js creates a global AMD environment.
        // Highcharts detects AMD and registers as a module instead of setting window.Highcharts.
        // We temporarily hide the AMD loader so Highcharts falls back to browser globals.
        var tempDefine = window.define;
        window.define = undefined;

        LoadCssResource("https://code.highcharts.com/css/highcharts.css", "highcharts-light", () => {
            LoadScriptResource("https://code.highcharts.com/highcharts.js", () => {
                LoadScriptResource("https://code.highcharts.com/modules/exporting.js", () => {
                    LoadScriptResource("https://code.highcharts.com/modules/export-data.js", () => {
                        // Drilldown support is required for the PieWithDrilldown chart
                        LoadScriptResource("https://code.highcharts.com/modules/drilldown.js", () => {
                            LoadScriptResource("https://code.highcharts.com/modules/accessibility.js", () => {
                                // Restore AMD loader now that Highcharts is loaded
                                window.define = tempDefine;

                                // Wait for Highcharts to be actually available before notifying Blazor
                                waitForHighcharts(() => {
                                    dotNetHelper.invokeMethodAsync("OnHighchartsLoaded");
                                });
                            });
                        });
                    });
                });
            });
        });
    }
}

// Polls for Highcharts availability; handles race condition where script.onload
// fires before the library has finished initializing its global object.
function waitForHighcharts(callback, attempts = 0) {
    if (typeof (Highcharts) == "object") {
        callback();
    } else if (attempts < 50) {
        // Retry up to 50 times (5 seconds max with 100ms intervals)
        setTimeout(() => waitForHighcharts(callback, attempts + 1), 100);
    } else {
        console.error("Highcharts failed to load after multiple attempts");
    }
}

function FileNameFromUrl(url) {
    var output = url;
    if (url != undefined && url != null && url != "") {
        var lastSlash = url.lastIndexOf("/");
        if (lastSlash > -1) {
            output = url.substring(lastSlash + 1);
        }
    }
    return output;
}

function LoadCssResource(url, existingClass, callback) {
    // Only load if the existingClass to test for is not found
    var exists = false;

    // Check to see if this script is already loaded.
    var scriptName = FileNameFromUrl(url);
    if (scriptName != undefined && scriptName != null && scriptName != "") {
        // Check to see if this script is already loaded.
        $("script").each(function () {
            var source = $(this).attr("src");
            if (source != undefined && source != null && source != "") {
                var sourceScriptName = FileNameFromUrl(source);
                if (sourceScriptName != undefined && sourceScriptName != null && sourceScriptName != "") {
                    if (scriptName.toLowerCase() == sourceScriptName.toLowerCase()) {
                        exists = true;
                    }
                }
            }
        });
    }

    if (exists) {
        if (callback != undefined && callback != null) {
            callback();
        }
    } else {
        var head = document.getElementsByTagName('head')[0];
        var link = document.createElement("link");
        link.href = url;
        link.type = "text/css";
        link.rel = "stylesheet";
        if (callback != undefined && callback != null) {
            link.onload = callback;
        }
        head.appendChild(link);
    }
}

function LoadScriptResource(url, callback) {
    var head = document.getElementsByTagName('head')[0];
    var script = document.createElement('script');
    script.type = 'text/javascript';
    script.src = url;
    if (callback != undefined && callback != null) {
        script.onload = callback;
    }
    head.appendChild(script);
}

export function SetDotNetHelper(value) {
    dotNetHelper = value;
}

export function RenderChart_Column(elementId, chartTitle, chartSubtitle, yAxisText, seriesCategories, seriesData) {
    Highcharts.chart(elementId, {
        chart: { type: 'column', styledMode: true },
        credits: { enabled: false },
        title: { text: chartTitle },
        subtitle: { text: chartSubtitle, x: -20, useHTML: true, },
        xAxis: { categories: seriesCategories, crosshair: true, },
        yAxis: { min: 0, title: { text: yAxisText } },
        legend: { enabled: false },
        series: seriesData,
        plotOptions: {
            series: {
                cursor: 'pointer',
                point: {
                    events: {
                        click: function () {
                            dotNetHelper.invokeMethodAsync("ChartItemClicked", this.index);
                        }
                    }
                }
            }
        }
    });
}

export function RenderChart_Pie(elementId, chartTitle, chartSubtitle, seriesData) {
    // Convert the seriesData to a simple javascript object.
    var chartData = [];
    for (var x = 0; x < seriesData.length; x++) {
        chartData.push([seriesData[x].name, seriesData[x].data]);
    }

    Highcharts.chart(elementId, {
        chart: { plotBackgroundColor: null, plotBorderWidth: null, plotShadow: false, type: 'pie', styledMode: true },
        credits: { enabled: false },
        title: { text: chartTitle, },
        subtitle: { text: chartSubtitle, x: -20, useHTML: true, },
        tooltip: {
            formatter: function () {
                var tooltip = seriesData[this.point.index].tooltip;
                return tooltip;
            },
            useHTML: true
        },
        accessibility: { point: { valueSuffix: '' } },
        plotOptions: {
            pie: {
                allowPointSelect: true,
                cursor: 'pointer',
                dataLabels: { enabled: true, format: '<b>{point.name}</b>' }
            }
        },
        series: [{
            name: 'Tickets',
            colorByPoint: true,
            data: chartData,
            point: {
                events: {
                    click: function () {
                        dotNetHelper.invokeMethodAsync("ChartItemClicked", this.index);
                    }
                }
            }
        }]
    });
}

// Helper: zoom hint
function withZoomHint(subtitleText, showHint) {
    if (!showHint) return subtitleText || '';
    var hint = (document.ontouchstart === undefined)
        ? ' — drag to zoom'
        : ' — pinch to zoom';
    return (subtitleText && subtitleText.length > 0) ? (subtitleText + hint) : ('Zoom' + hint);
}

// NEW: Zoomable datetime line chart with multi-series legend
export function RenderChart_LineTimeSeries(elementId, chartTitle, chartSubtitle, yAxisText, seriesData, showHint) {
    Highcharts.chart(elementId, {
        chart: { zoomType: 'x', styledMode: true },
        credits: { enabled: false },
        title: { text: chartTitle },
        subtitle: {
            text: withZoomHint(chartSubtitle, showHint),
            x: -20,
            useHTML: true
        },
        xAxis: { type: 'datetime' },
        yAxis: { title: { text: yAxisText } },
        legend: {
            enabled: true,
            layout: 'vertical',
            align: 'right',
            verticalAlign: 'middle'
        },
        tooltip: {
            shared: true,
            xDateFormat: '%Y-%m-%d',
            valueDecimals: 0
        },
        plotOptions: {
            series: {
                turboThreshold: 50000,
                marker: { enabled: false, radius: 2 },
                lineWidth: 1,
                cursor: 'pointer',
                // parity with old configs
                label: { connectorAllowed: false },
                point: {
                    events: {
                        click: function () {
                            dotNetHelper.invokeMethodAsync("ChartItemClicked", this.index);
                        }
                    }
                }
            }
        },
        series: seriesData,
        responsive: {
            rules: [{
                condition: { maxWidth: 600 },
                chartOptions: {
                    legend: { layout: 'horizontal', align: 'center', verticalAlign: 'bottom' }
                }
            }]
        }
    });
}

// NEW: Pie with Drilldown (Incoming/Outgoing → breakdown)
export function RenderChart_PieWithDrilldown(elementId, chartTitle, chartSubtitle, rootItems, drilldownItems, sortDescending) {
    // Map root items to Highcharts format
    var rootData = (rootItems || []).map(function (x) {
        return { name: x.name, y: Number(x.data), drilldown: x.name };
    });

    if (sortDescending) {
        rootData.sort(function (a, b) { return b.y - a.y; });
    }

    // Map drilldown series
    var ddSeries = (drilldownItems || []).map(function (s) {
        var pts = (s.data || []).map(function (p) { return [p.name, Number(p.y)]; });
        if (sortDescending) {
            pts.sort(function (a, b) { return Number(b[1]) - Number(a[1]); });
        }
        return { id: s.id, name: s.id, data: pts };
    });

    Highcharts.chart(elementId, {
        chart: { type: 'pie', styledMode: true },
        credits: { enabled: false },
        title: { text: chartTitle },
        subtitle: { text: chartSubtitle, x: -20, useHTML: true },
        accessibility: {
            announceNewData: { enabled: true },
            point: { valueSuffix: '%' }
        },
        plotOptions: {
            series: {
                borderRadius: 5,
                dataLabels: { enabled: true, format: '{point.name}: {point.percentage:.1f}%<br/>({point.y}/{point.total})' }
            }
        },
        tooltip: {
            headerFormat: '<span style="font-size:11px">{series.name}</span><br/>',
            pointFormat: '<span>{point.name}</span>: <b>{point.percentage:.2f}%</b> ({point.y}/{point.total})'
        },
        series: [{
            name: 'Direction',
            colorByPoint: true,
            data: rootData,
            point: {
                events: {
                    click: function () {
                        // send index of the point in the current (root) series
                        dotNetHelper.invokeMethodAsync("ChartItemClicked", this.index);
                    }
                }
            }
        }],
        drilldown: {
            series: ddSeries
        }
    });
}

// NEW: Column time-series (datetime) with zoom
export function RenderChart_ColumnTimeSeries(elementId, chartTitle, chartSubtitle, yAxisText, seriesData, showHint) {
    Highcharts.chart(elementId, {
        chart: { type: 'column', zoomType: 'x', styledMode: true },
        credits: { enabled: false },
        title: { text: chartTitle },
        subtitle: { text: withZoomHint(chartSubtitle, showHint), x: -20, useHTML: true },
        xAxis: { type: 'datetime' },
        yAxis: { title: { text: yAxisText } },
        legend: {
            enabled: true,
            layout: 'vertical',
            align: 'right',
            verticalAlign: 'middle'
        },
        tooltip: {
            shared: true,
            xDateFormat: '%Y-%m-%d',
            valueDecimals: 0
        },
        plotOptions: {
            series: {
                label: { connectorAllowed: false },
                point: {
                    events: {
                        click: function () {
                            dotNetHelper.invokeMethodAsync("ChartItemClicked", this.index);
                        }
                    }
                }
            }
        },
        series: seriesData,
        responsive: {
            rules: [{
                condition: { maxWidth: 600 },
                chartOptions: {
                    legend: { layout: 'horizontal', align: 'center', verticalAlign: 'bottom' }
                }
            }]
        }
    });
}

// NEW: Area totals (gradient, legend disabled) with zoom
export function RenderChart_AreaTotals(elementId, chartTitle, chartSubtitle, yAxisText, seriesData, showHint) {
    // Ensure first series is area with desired styling
    var prepared = (seriesData || []).map(function (s, idx) {
        var copy = Object.assign({}, s);
        if (idx === 0) {
            copy.type = 'area';
            copy.marker = { radius: 2 };
            copy.lineWidth = 1;
            copy.states = { hover: { lineWidth: 1 } };
            copy.threshold = null;
            // Gradient fill (top solid to transparent bottom). Let CSS colors style series; here we rely on default fill fallback.
            copy.fillColor = {
                linearGradient: { x1: 0, y1: 0, x2: 0, y2: 1 },
                stops: [
                    [0, 'rgba(68, 114, 196, 0.6)'],
                    [1, 'rgba(68, 114, 196, 0.0)']
                ]
            };
        }
        return copy;
    });

    Highcharts.chart(elementId, {
        chart: { zoomType: 'x', styledMode: true },
        credits: { enabled: false },
        title: { text: chartTitle },
        subtitle: { text: withZoomHint(chartSubtitle, showHint), x: -20, useHTML: true },
        xAxis: { type: 'datetime' },
        yAxis: { title: { text: yAxisText } },
        legend: { enabled: false },
        tooltip: {
            shared: true,
            xDateFormat: '%Y-%m-%d',
            valueDecimals: 0
        },
        series: prepared
    });
}
