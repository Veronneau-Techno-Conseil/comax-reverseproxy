var browserify = require('browserify');
var tsify = require('tsify');
var fs = require('fs');

browserify()
    .add(['./node_modules/jquery/dist/jquery.min.js', './node_modules/select-picker/dist/picker.min.js'], {external : true })
    .add('./assets/ts/site.ts') // main entry of an application
    .plugin(tsify, { noImplicitAny: true })
    //.plugin(watchify)
    .bundle()
    .on('error', function (error) { console.error(error.toString()); })
    .pipe(fs.createWriteStream("./wwwroot/site.js"));
