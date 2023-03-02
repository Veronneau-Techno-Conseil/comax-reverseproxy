var gulp = require('gulp'),
    sass = require('gulp-sass')(require('sass'))
    cssmin = require("gulp-cssmin")
    rename = require("gulp-rename");

gulp.task('min', function (done) {
    gulp.src('assets/sass/styles.scss')
        .pipe(sass({
            includePaths: ['node_modules/@vertechcon/comax-styles/dist/']
        }).on('error', sass.logError))
        .pipe(cssmin())
        .pipe(rename({
            suffix: ".min"
        }))
        .pipe(gulp.dest('wwwroot'));
    done();
});

gulp.task("serve", gulp.parallel(["min"]));
gulp.task("default", gulp.series("serve"));
