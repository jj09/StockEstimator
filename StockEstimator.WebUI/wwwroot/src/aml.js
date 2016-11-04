import {inject} from 'aurelia-framework';
import {HttpClient, json} from 'aurelia-fetch-client';
import 'jquery-ui';

@inject(HttpClient)
export class Aml {
    estimateFor = '2016-12-12';
    estimatedPrice = '?';
    experiment = 'msft';

    experiments = ['msft', 'msft(2015-11-04 to 2016-11-03)', 'msft(2016-10-04 to 2016-11-03)'];

    constructor(http) {
        http.configure(config => {
        config
            .useStandardConfiguration()
            .withBaseUrl('http://localhost:8083/');
        });

        this.http = http;
    }

    getPrice() {
        let body = {
            Inputs: {
                input1: [{Date: this.estimateFor}]
            }
        };

        this.http.fetch(`GetPriceFromAzure?date=${this.estimateFor}&experiment=${this.experiment}`)
            .then(response => response.json())
            .then(data => {
                this.estimatedPrice = JSON.parse(data).Results.output1[0]['Scored Labels'];                
            });
    }

    attached() {
        $("#estimate-for").datepicker({
            onSelect: (selected, evt) => {
                this.till = selected.toString();
            }
        });
    }
}

export class CurrencyValueConverter {
  toView(value) {
    if (value === '?') {
        return value;
    }

    return value && '$' + parseFloat(Math.round(value * 100) / 100).toFixed(2);
  }
}