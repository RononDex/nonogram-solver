var output = "";
var table_Rows_Selector = "#cross_left";
var table_Columns_Selector = "#cross_top";
output +=
  $(table_Columns_Selector + " tr:first td").length +
  " " +
  $(table_Rows_Selector + " tr").length +
  "\n";

// Rows
$(table_Rows_Selector + " tr").each((element, obj) => {
  $(obj)
    .find("td")
    .toArray()
    .forEach((element, idx, array) => {
      var text = $(element).text();
      if (text != "") {
        output += text;
        if (idx != array.length - 1) {
          output += " ";
        }
      }
    });

  output += "\n";
});

// Columns
var columns = [];
for (var i = 0; i < $(table_Columns_Selector + " tr:first td").length; i++) {
  columns[i] = [];
}

var col = 0;
var row = 0;

$(table_Columns_Selector + " tr")
  .toArray()
  .forEach((element) => {
    $(element)
      .find("td")
      .toArray()
      .forEach((element1) => {
        console.log(element1);
        var text = $(element1).text();
        columns[col][row] = text;
        col++;
      });
    col = 0;
    row++;
  });

for (var i = 0; i < $(table_Columns_Selector + " tr:first td").length; i++) {
  for (var j = 0; j < columns[i].length; j++) {
    if (columns[i][j] != "") {
      output += columns[i][j];
      if (j != columns[i].length - 1) {
        output += " ";
      }
    }
  }

  output += "\n";
}

console.log(output);
