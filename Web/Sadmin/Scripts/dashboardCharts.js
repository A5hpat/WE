var currencySymbol = $('#currencySymbol').val();

function updateOrdersChart(length) {
	d3.json("dashboardstatistics.axd?type=orders&length=" + length, function (error, data) {
		var orderData = [
			{
				values: data.data,
				key: 'Revenue',
				color: '#0EC1FC'
			}
		];

		nv.addGraph(function () {
			var chart = nv.models.lineChart()
				.margin({ left: 100, right: 50 })
				.showLegend(false)
				.x(function (d, i) { return i })
				.y(function (d, i) { return d[1] })
			;

			chart.xAxis
				.axisLabel('Time')
				.tickFormat(function (d) {
					var dx = orderData[0].values[d] && orderData[0].values[d][0] || 0;
					return dx;
				});

			chart.yAxis
				.axisLabel('Revenue')
				.tickFormat(function (d) { return currencySymbol + d3.format(',f')(d) });

			chart.forceY([0]);

			d3.select('#ordersChart svg')
				.datum(orderData)
				.call(chart);

			nv.utils.windowResize(function () { chart.update() });
			return chart;
		});
	});
}

function updateProductsChart(length) {
	d3.json("dashboardstatistics.axd?type=products&length=" + length, function (error, data) {
		var productData = [
			{
				values: data.data,
				key: 'Revenue',
				color: '#0EC1FC'
			}
		];

		var testData = data.data;

		nv.addGraph(function () {
			var chart = nv.models.discreteBarChart()
				.margin({ left: 100, right: 50 })
				.x(function (d) { return d[2] })
				.y(function (d) { return d[1] })				
				.staggerLabels(false)
				.tooltips(true)
				.tooltipContent(function (yLabel, y, x, key) {
					return '<p class="product-graph-tooltip">' + testData[key.pointIndex][0] + '</p>';
				})				
				.showValues(true)
				.transitionDuration(350)
			;

			chart.xAxis
				.axisLabel('Product ID');

			chart.yAxis
				.axisLabel('Revenue')
				.tickFormat(function (d) { return currencySymbol + d3.format(',f')(d) });

			d3.select('#productsChart svg')
				.datum(productData)
				.call(chart);

			nv.utils.windowResize(chart.update);

			return chart;
		});
	});
}

//setup the onchange events for the chart length dropdowns
var ordersLength = $('#orders-length');
ordersLength.change(function () {
	updateOrdersChart($(this).val());
});

var productsLength = $('#products-length');
productsLength.change(function () {
	updateProductsChart($(this).val());
});

//initialize the charts
updateOrdersChart(ordersLength.val());
updateProductsChart(productsLength.val())