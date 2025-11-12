// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
_wait = false;
/*_ProdnStat = [];*/
_Line = 12;
let preparationScan = []
let bulkyScan = []
ModelCode = "";

function LoadFrame(_frame, _label) {
    var myIcons = $('#' + _frame)
    _actPage = _frame
    var _myLabel = myIcons.children().find("p").text()

    clearTimeout(_loadInterval)
    clearTimeout(_wait)
    $('#myContent').removeClass("animate__animated animate__fadeOutLeft")
    $('.main-panel').scrollTop(0)
    $('#myContent').html('').load("/home/" + _frame, function (response, status, xhr) {
        if (status == "error") {
            //location.reload();
            console.log(xhr)
        } else {
            ActivateScroll();
            var myIcons = $('#' + _past)
            document.title = _frame + "-Production Monitoring"
            // console.log(_label)
            if (_label != null) {
                myIcons.children().find("p").text(_label).removeClass('animate__flash animate__infinite').addClass('animate__flipInX')
                _loadInterval = setTimeout(() => {
                    clearTimeout(_loadInterval)
                    myIcons.children().find("p").removeClass('animate__animated animate__flipInX')
                }, 250);
            }
            //if (_myLabel == "Loading") {
            //    location.reload();
            //}
            $('#myContent').fadeIn("slow");
        }
    })
}

function genChart(att, scale, time) {
    $.ajax({
        url: '/home/genChart',
        data: {
            'att': att,
            'scale': scale,
            'time': time,
        },
        success: function (data) {
            console.log(data)
            myChart(data)            
        }
    });
}

function myChart(data) {
    var ctx = document.getElementById("mainChart").getContext("2d");
    var gradientStroke = ctx.createLinearGradient(0, 230, 0, 50);

    gradientStroke.addColorStop(1, 'rgba(72,72,176,0.2)');
    gradientStroke.addColorStop(0.3, 'rgba(72,72,176,0.1)');
    gradientStroke.addColorStop(0, 'rgba(239,207,227,0.6)'); //purple colors(239, 207, 227);, 1

    const totalDuration = 10000;
    const delayBetweenPoints = Object.keys(data.data).length;
    const previousY = (ctx) => ctx.index === 0 ? ctx.chart.scales.y.getPixelForValue(1000) : ctx.chart.getDatasetMeta(ctx.datasetIndex).data[ctx.index - 1].getProps(['y'], true).y;
    const animation = {
        x: {
            type: 'number',
            easing: 'linear',
            duration: delayBetweenPoints,
            from: NaN, // the point is initially skipped
            delay(ctx) {
                if (ctx.type !== 'data' || ctx.xStarted) {
                    return 0;
                }
                ctx.xStarted = true;
                return ctx.index * delayBetweenPoints;
            }
        },
        y: {
            type: 'number',
            easing: 'linear',
            duration: delayBetweenPoints,
            from: previousY,
            delay(ctx) {
                if (ctx.type !== 'data' || ctx.yStarted) {
                    return 0;
                }
                ctx.yStarted = true;
                return ctx.index * delayBetweenPoints;
            }
        }
    };

    var data = {
        labels: Object.keys(data.data),
        datasets: [{
            label: "Data",
            fill: true,
            backgroundColor: gradientStroke,
            borderColor: '#d048b6',
            borderWidth: 2,
            borderDash: [],
            borderDashOffset: 0.0,
            pointBackgroundColor: '#d048b6',
            pointBorderColor: 'rgba(255,255,255,0)',
            pointHoverBackgroundColor: '#d048b6',
            pointBorderWidth: 20,
            pointHoverRadius: 4,
            pointHoverBorderWidth: 15,
            pointRadius: 4,
            tension: 0.4,
            data: Object.values(data.data),
        }]
    };

    if (window.mainChart2 != null) {
        window.mainChart2.destroy();
    };
    window.mainChart2 = new Chart(ctx, {
        type: 'line',
        data: data,
        options: {
            maintainAspectRatio: false,
            // animation,
            interaction: {
                intersect: false
            },
            plugins: {
                legend: {
                    display: false,
                }
            },
            tooltips: {
                backgroundColor: '#f5f5f5',
                titleFontColor: '#333',
                bodyFontColor: '#666',
                bodySpacing: 4,
                xPadding: 12,
                mode: "nearest",
                intersect: 0,
                position: "nearest"
            },
            responsive: true,
            scales: {
                y: {
                    title: {
                        display: true,
                        text: '%'
                    },
                    barPercentage: 1.6,
                    grid: {
                        drawBorder: false,
                        color: 'rgba(29,140,248,0.0)',
                        zeroLineColor: "transparent",
                    },
                    ticks: {
                        suggestedMin: 60,
                        suggestedMax: 125,
                        padding: 20,
                        fontColor: "#9a9a9a"
                    }
                },
                x: {
                    // title: {
                    //     display: true,
                    //     text: 'Date'
                    // },
                    barPercentage: 1.6,
                    grid: {
                        drawBorder: false,
                        color: 'rgba(225,78,202,0.1)',
                        zeroLineColor: "transparent",
                    },
                    ticks: {
                        padding: 20,
                        fontColor: "#9a9a9a"
                    }
                },
            },
            onClick: (event, elements, chart) => {
                if (elements[0]) {
                    const i = chart.data.datasets[elements[0].datasetIndex].label;
                    const j = chart.data.labels[elements[0].index]
                    CustomOnClickHandler(j);
                    // genChart("specify","daily",j+"/2024");

                }
            }
        },
    });
}
document.addEventListener("DOMContentLoaded", () => {
    const adminMenu = document.getElementById("Admin");

    if (adminMenu) {
        adminMenu.addEventListener("click", function (e) {
            e.preventDefault();

            // 🔽 Load isi halaman admin ke container
            $("#main-content").load("/Home/Admin", function () {
                // 🔄 Baru setelah isi dimuat, tombol-tombol bisa dikenali
                if (typeof initializeAdminManagement === "function") {
                    initializeAdminManagement();
                }
            });
        });
    }
});

const wrapper = document.getElementById("wrapper");
const content = document.getElementById("page-content-wrapper");

document.getElementById("menu-toggle").addEventListener("click", function () {
    wrapper.classList.toggle("toggled");
    content.classList.toggle("content-shifted");
});
document.addEventListener('DOMContentLoaded', function () {
    const sidebar = document.querySelector('.sidebar');
    const overlay = document.getElementById('sidebarOverlay');
    const toggleButton = document.getElementById('sidebarToggle');

    toggleButton?.addEventListener('click', function () {
        sidebar.classList.toggle('show');
        overlay.classList.toggle('show');
    });

    overlay?.addEventListener('click', function () {
        sidebar.classList.remove('show');
        overlay.classList.remove('show');
    });
});

function ActivateScroll() {
    $('.table-responsive').each(function () {
        // console.log($(this)[0])
        var ps = new PerfectScrollbar($(this)[0]);
    });
}

function GetToken() {
    return document.querySelector("[name='__RequestVerificationToken']").value;
}
//document.addEventListener('DOMContentLoaded', function () {
//    const rep = document.getElementById('reporter');
//    if (rep) {
//        rep.setAttribute('readonly', 'readonly'); 
//        rep.addEventListener('keydown', (e) => e.preventDefault());
//        rep.addEventListener('paste', (e) => e.preventDefault());
//        rep.addEventListener('input', (e) => e.preventDefault());
//    }
//});
