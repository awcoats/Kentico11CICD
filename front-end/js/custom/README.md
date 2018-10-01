# custom

This is where all custom scripts shuld be placed. Custom scripts are scripts that you have written and/or modified to the point they cannot updated if they originated from a third party

1. Place individual custom scripts in this directory.
2. Name custom scripts intuitively based on use of associated element or intent
i.e. - "accordion", "mainMenu", "iosScrollFix", etc.
3. Name should always begin with an underscore:
i.e. - "_accordion.js"
4. Open "imports.js" and include the custom script using this format:
//=require _accordion.js 
5. Your script will be pulled into the frontEnd.js file in the order you specify within the imports.js file