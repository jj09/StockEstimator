import {inject} from 'aurelia-framework';
import {HttpClient} from 'aurelia-fetch-client';
import {bindable} from 'aurelia-framework';
import 'fetch';
import 'jquery-ui';

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

    // https://twitter.com/stevensanderson/status/580782729991745536
    setTimeout(() => {
      $("#date-till").datepicker();
      $("#date-till").datepicker("setDate", new Date(this.till));
      $("#date-till").blur(() => this.getData());
      $("#date-since").datepicker();
      $("#date-since").datepicker("setDate", new Date(this.since));
      $("#date-since").blur(() => this.getData());
    }, 1500);
  }

  propertyChanged(propertyName, newValue, oldValue) { 
    if (["ticker", "till", "since"].indexOf(propertyName) !== -1) {
      this.getData();
    }
  }

  getData() {
    console.log('getting data for', this.ticker, ' for', this.till, 'based on data since', this.since);
    return this.http.fetch(`GetPriceForDateRange?ticker=${this.ticker}&since=${this.since}&till=${this.till}`)
      .then(response => response.json())
      .then(prices => this.estimatedPrice = prices[prices.length-1].item2);
  }

  activate() {
    return this.getData();
  }
}

export class CurrencyValueConverter {
  toView(value) {
    return value && parseFloat(Math.round(value * 100) / 100).toFixed(2);
  }
}