var dotNetHelper;

export function LoadHighchartsResources() {
    // This function checks to see if the Highcharts libraries have been loaded.
    // If so, this immediately invokes the OnHighchartsLoaded method in the Blazor
    // component. If not, the libraries are loaded and then the method is invoked.

    if(typeof(Highcharts) == "object"){
        dotNetHelper.invokeMethodAsync("OnHighchartsLoaded");
    } else {
        LoadCssResource("https://code.highcharts.com/css/highcharts.css", "highcharts-light", () => {
            LoadScriptResource("https://code.highcharts.com/highcharts.js", () => {
                LoadScriptResource("https://code.highcharts.com/modules/exporting.js", () => {
                    LoadScriptResource("https://code.highcharts.com/modules/export-data.js", () => {
                        LoadScriptResource("https://code.highcharts.com/modules/accessibility.js", () => {
                            dotNetHelper.invokeMethodAsync("OnHighchartsLoaded");
                        });
                    });
                });
            });
        });
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
    //console.log("RenderChart_Column", elementId, chartTitle, chartSubtitle, yAxisText, seriesCategories, seriesData);

    Highcharts.chart(elementId, {
        chart: {
            type: 'column',
            styledMode: true
        },
        credits: { enabled: false },
        title: { text: chartTitle },
        subtitle: {
            text: chartSubtitle,
            x: -20,
            useHTML: true,
        },
        xAxis: {
            categories: seriesCategories,
            crosshair: true,
        },
        yAxis: {
            min: 0,
            title: {
                text: yAxisText
            }
        },
        legend: { enabled: false },
        series: seriesData,
        plotOptions: {
            series: {
                cursor: 'pointer',
                point: {
                    events: {
                        click: function (event) {
                            dotNetHelper.invokeMethodAsync("ChartItemClicked", this.index);
                        }
                    }
                }
            }
        }
    });
}

export function RenderChart_Pie(elementId, chartTitle, chartSubtitle, seriesData) {
    //console.log("RenderChart_Pie", elementId, chartTitle, chartSubtitle, seriesData);

    // Convert the seriesData to a simple javascript object.
    var chartData = [];
    for (var x = 0; x < seriesData.length; x++) {
        chartData.push([seriesData[x].name, seriesData[x].data]);
    }

    Highcharts.chart(elementId, {
        chart: {
            plotBackgroundColor: null,
            plotBorderWidth: null,
            plotShadow: false,
            type: 'pie',
            styledMode: true
        },
        credits: { enabled: false },
        title: {
            text: chartTitle,
        },
        subtitle: {
            text: chartSubtitle,
            x: -20,
            useHTML: true,
        },
        tooltip: {
            formatter: function () {
                var tooltip = seriesData[this.point.index].tooltip;
                return tooltip;
            },
            useHTML: true
        },
        accessibility: {
            point: {
                valueSuffix: ''
            }
        },
        plotOptions: {
            pie: {
                allowPointSelect: true,
                cursor: 'pointer',
                dataLabels: {
                    enabled: true,
                    format: '<b>{point.name}</b>'
                }
            }
        },
        series: [{
            name: 'Tickets',
            colorByPoint: true,
            data: chartData,
            point: {
                events: {
                    click: function (event) {
                        dotNetHelper.invokeMethodAsync("ChartItemClicked", this.index);
                    }
                }
            }
        }]
    });
}