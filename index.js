var fs = require('fs');

function generateString(letters) {
    var result = '';
    var possible = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';

    for (var i = 0; i < letters; ++i) result += possible.charAt(Math.floor(Math.random() * possible.length));
    return result;
};

function addLine(index) {
    if (index >= 100000000) {
        console.log('File complete')
        process.exit(0);
    }

    var row = [];
    for (var j = 0; j < 100; ++j) {
        row.push(generateString(Math.floor(Math.random() * 20) + 5));
    }

    fs.appendFile('large.csv', row.join(',') + '\n', function (err) {
        console.log(`Finished appending line ${index}`);
        addLine(++index);
    });
};

addLine(0);

// var fs = require('fs');
// var data = [];
// for (var i = 0; i < 1000; ++i) {
//     data.push(`[CsvProperty(${i})]`);
//     data.push(`public string Row${i + 1} { get; set; }`);
// }

// fs.writeFile('test.cs', '       ' + data.join('\n       ') + '\n', function(err) {
//     if (err) console.warn(err);
//     process.exit(1);
// });