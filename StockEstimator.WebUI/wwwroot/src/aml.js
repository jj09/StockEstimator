import {inject} from 'aurelia-framework';
import {HttpClient, json} from 'aurelia-fetch-client';
import 'jquery-ui';

@inject(HttpClient)
export class Aml {
    estimateFor = "2016-12-12";
    estimatedPrice = "?";

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

        this.http.fetch(`GetPriceFromAzure?date=${this.estimateFor}`)
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