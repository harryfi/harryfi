# -*- coding: utf-8 -*-
import scrapy
from scrapy.http import FormRequest

class LoginSpider(scrapy.Spider):
    name = 'login'
    allowed_domains = ['dev.masteronline.co.id']
    start_urls = ['https://dev.masteronline.co.id/scrapper/login/']

    email = ''
    password = ''

    def parse(self, response):
        req_token = response.xpath('//*[@name="__RequestVerificationToken"]/@value').extract_first()
        
        yield FormRequest('https://dev.masteronline.co.id/scrapper/loggingin', 
            formdata={'__RequestVerificationToken': req_token, 'Email': self.email, 'Password': self.password}, 
            callback=self.parse_after_login)

    def parse_after_login(self, response):
        if response.xpath('//title[text()="Data Barang"]'):
            self.log('You logged in!')
            array_barang = response.xpath('//tr[@class="barang"]')

            for barang in array_barang:
                kode_barang = barang.xpath('.//*[@class="kode_barang"]/text()').extract_first()
                nama_barang = barang.xpath('.//*[@class="nama_barang"]/text()').extract_first()
                kategori_barang = barang.xpath('.//*[@class="kategori_barang"]/text()').extract_first()
                merk_barang = barang.xpath('.//*[@class="merk_barang"]/text()').extract_first()
                hargajual_barang = barang.xpath('.//*[@class="hargajual_barang"]/text()').extract_first()

                yield {
                    'Kode': kode_barang,
                    'Nama': nama_barang,
                    'Kategori': kategori_barang,
                    'Merk': merk_barang,
                    'Harga Jual': hargajual_barang,
                }
        else:
            self.log('Gagal login!')
            