import {inject} from 'aurelia-framework';
import {HttpClient} from 'aurelia-fetch-client';
import {bindable} from 'aurelia-framework';
import 'fetch';
import 'jquery-ui';
import 'd3';

@inject(HttpClient)
export class Home {
  heading = 'Estimating future stock prices';

  @bindable ticker = "msft";
  @bindable till = "2016-03-11";
  @bindable since = "2015-01-11";
  tickers = ["msft", "googl", "amzn", "aapl"];

  constructor(http) {
    http.configure(config => {
      config
        .useStandardConfiguration()
        .withBaseUrl('http://localhost:8083/');
    });

    this.http = http;
  }

  propertyChanged(propertyName, newValue, oldValue) { 
    if (["ticker", "till", "since"].indexOf(propertyName) !== -1) {
      this.getData();
    }
  }

  getData() {
    console.log('getting future', this.ticker, 'prices till', this.till, 'based on data since', this.since);
    return this.http.fetch(`GetPriceForDateRange?ticker=${this.ticker}&since=${this.since}&till=${this.till}`)
      .then(response => response.json())
      .then(prices => {
        this.prices = prices;
        this.estimatedPrice = prices[prices.length-1].item2;
      })
      .then(() => this.drawChart());
  }

  activate() {
    //return this.getData();
  }

  // the component is attached to the DOM (in document). 
  // If the view-model has an attached callback, it will be invoked at this time.
  // http://aurelia.io/docs.html#/aurelia/framework/1.0.0-beta.1.1.3/doc/article/cheat-sheet
  attached() {
    $("#date-till").datepicker();
    $("#date-till").datepicker("setDate", new Date(this.till));
    $("#date-till").blur(() => this.getData());
    $("#date-since").datepicker();
    $("#date-since").datepicker("setDate", new Date(this.since));
    $("#date-since").blur(() => this.getData());

    this.getData();
  }

  drawChart() {
    let data = this.prices.map(p => {
      return { 
        price: p.item2, 
        date: new Date(p.item1)
      };
    });
    $("#visualisation").empty();
    let vis = d3.select("#visualisation");
    const WIDTH = 600;
    const HEIGHT = 500;
    const MARGINS = {
        top: 20,
        right: 20,
        bottom: 20,
        left: 50
      };
    let xScale = d3.time.scale().range([MARGINS.left, WIDTH - MARGINS.right]).domain(d3.extent(data, function(d) { return d.date; }));
    let yScale = d3.scale.linear().range([HEIGHT - MARGINS.top, MARGINS.bottom]).domain(d3.extent(data, function(d) { return d.price; }));
    let xAxis = d3.svg.axis().scale(xScale);
    let yAxis = d3.svg.axis().scale(yScale).orient("left");
    
    vis.append("svg:g")
        .attr("class", "x axis")
        .attr("transform", "translate(0," + (HEIGHT - MARGINS.bottom) + ")")
        .call(xAxis);
    vis.append("svg:g")
        .attr("class", "y axis")
        .attr("transform", "translate(" + (MARGINS.left) + ",0)")
        .call(yAxis);
    let lineGen = d3.svg.line()
        .x(function(d) {
            return xScale(d.date);
        })
        .y(function(d) {
            return yScale(d.price);
        })
        .interpolate("basis");
    vis.append('svg:path')
        .datum(data)
        .attr("class", "line")
        .attr("d", lineGen)
        .attr('stroke', 'green')
        .attr('stroke-width', 2)
        .attr('fill', 'none');
  }
}

export class CurrencyValueConverter {
  toView(value) {
    return value && parseFloat(Math.round(value * 100) / 100).toFixed(2);
  }
}