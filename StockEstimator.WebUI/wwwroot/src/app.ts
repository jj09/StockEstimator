export class App {
  router: any;
  
  configureRouter(config, router) {
    config.title = 'Stock Estimator';
    config.map([
      { route: ['', 'home'],    name: 'home',         moduleId: 'home',         nav: true, title: 'Home' },
      { route: 'aml',         name: 'aml',         moduleId: 'aml',         nav: true, title: 'Azure Machine Learning' },
      { route: 'about',    		name: 'about',        moduleId: 'about',        nav: true, title: 'About' },
    ]);

    this.router = router;
  }
}
